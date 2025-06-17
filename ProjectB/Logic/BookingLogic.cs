using Microsoft.VisualBasic;
using ProjectB.DataAccess;
using Spectre.Console;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
public static class BookingLogic
{
    public static IBookingAccess BookingAccessService { get; set; } = new BookingAccess();
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static void BackfillFlightSeats(int FlightID)
    {
        FlightModel flight = FlightAccessService.GetById(FlightID);
        AirplaneModel airplane = AirplaneLogic.GetAirplaneByID(flight.AirplaneID);

        // If flight and airplane exist, and there are no seats for the flight, create them
        if (flight != null && airplane != null && !FlightSeatAccessService.HasAnySeatsForFlight(FlightID))
        {
            for (int i = 1; i < airplane.TotalSeats; i++)
            {
                FlightSeatAccessService.CreateFlightSeats(FlightID, flight.AirplaneID);
            }
        }
    }

    public static (decimal finalPrice, decimal discount) CalculateBookingPrice(User user, FlightModel flight, SeatModel seat, int amountLuggage, bool isInsurance, (bool, bool) coupon)
    {
        decimal finalPrice = (decimal)seat.Price;
        decimal totalDiscount = 1.0m;

        // Add luggage cost
        if (amountLuggage > 0)
        {
            finalPrice += 50 * amountLuggage;
        }

        // Apply discounts
        if (user.FirstTimeDiscount)
        {
            totalDiscount -= 0.1m; // 10% discount
        }

        if (DateTime.Now >= user.BirthDate.AddYears(65) && !user.Guest)
        {
            totalDiscount -= 0.2m; // 20% discount
        }

        if (coupon.Item1) // isValidCoupon
        {
            totalDiscount -= 0.05m; // 5% discount 
        }

        if (coupon.Item2) // isSpice
        {
            return (-1 * finalPrice * totalDiscount, totalDiscount); // is minus, pay with spice (easter egg)
        }

        return (finalPrice * totalDiscount, totalDiscount);
    }

    public static BookingModel BookingBuilder(User user, FlightModel flight, SeatModel seat, (bool, bool) coupon, int amountLuggage = 0, bool insuranceStatus = false)
    {
        (decimal finalPrice, decimal discount) calculatedPrice = CalculateBookingPrice(user, flight, seat, amountLuggage, insuranceStatus, coupon);

        var booking = new BookingModel
        {
            BookingStatus = "Pending",
            UserID = user.UserID,
            PassengerFirstName = user.FirstName,
            PassengerLastName = user.LastName,
            PassengerEmail = user.EmailAddress,
            PassengerPhone = user.PhoneNumber,
            FlightID = flight.FlightID,
            Airline = flight.Airline,
            AirplaneModel = flight.AirplaneID,
            DepartureAirport = flight.DepartureAirport,
            ArrivalAirport = flight.ArrivalAirport,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            SeatID = seat.SeatID,
            SeatClass = seat.SeatType,
            LuggageAmount = amountLuggage,
            HasInsurance = insuranceStatus,
            Discount = calculatedPrice.discount,
            TotalPrice = calculatedPrice.finalPrice
        };
        return booking;
    }

    public static List<BookingModel> GetBookingsForUser(int userId, bool upcoming)
    {
        List<BookingModel> all = BookingAccessService.GetBookingsByUser(userId);
        DateTime now = DateTime.Now;
        return all.Where(b =>
        {
            if (b.BookingStatus == "Cancelled")
                return !upcoming;
            return upcoming ? b.DepartureTime >= now : b.DepartureTime < now;
        }).ToList();
    }

    public static int GetNextBookingId()
    {
        var all = BookingAccessService.GetBookingsByUser(0);
        return all.Any() ? all.Max(b => b.BookingID) + 1 : 1;
    }

    public static bool CancelBooking(int bookingId)
    {
        BookingModel booking = BookingAccessService.GetBookingById(bookingId);
        if (booking == null)
        {
            AnsiConsole.MarkupLine($"[red]Booking with ID {bookingId} not found.[/]");
            return false;
        }

        // Free up the seat
        FlightModel flight = FlightAccessService.GetById(booking.FlightID);
        if (flight != null)
        {
            FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, booking.SeatID, false);
        }
        if (booking.HasInsurance)
        {
            // Full refund, no fee
            booking.TotalPrice = 0;
            AnsiConsole.MarkupLine("[green]Booking cancelled. Full refund issued due to insurance.[/]");
        }
        else
        {
            // Apply cancellation fee (e.g., $100)
            booking.TotalPrice = Math.Max(0, booking.TotalPrice - 100);
            AnsiConsole.MarkupLine("[yellow]A cancellation fee of $100 has been applied.[/]");
        }
            // Update the booking in db
            booking.BookingStatus = "Cancelled";
            BookingAccessService.UpdateBooking(booking);

            AnsiConsole.MarkupLine($"[green]Booking with ID {bookingId} has been cancelled.[/]");
            return true;
        }

    public static bool ModifyBooking(int bookingId, string newSeatId, int newLuggageAmount)
    {
        BookingModel booking = BookingAccessService.GetBookingById(bookingId);
        if (booking == null)
        {
            return false;
        }

        FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, booking.SeatID, false);
        FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, newSeatId, true);

        booking.SeatID = newSeatId;
        booking.LuggageAmount = newLuggageAmount;
        BookingAccessService.UpdateBooking(booking);

        return true;
    }

    public static void BookTheDamnFlight(BookingModel booking)
    {
        booking.BookingStatus = "Confirmed";
        BookingAccessService.AddBooking(booking);
    }

    public static Spectre.Console.Rendering.IRenderable CreateBookingTable(List<BookingModel> bookings)
    {
        if (bookings == null || !bookings.Any())
        {
            var panel = new Panel("[yellow]No bookings found.[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(errorStyle);
            return panel;
        }

        Spectre.Console.Table table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();
        table.AddColumns("BookingID", "Status", "FlightID", "Seat", "Class", "Passenger", "Departure", "Arrival");

        foreach (var booking in bookings)
        {
                table.AddRow(
                booking.BookingID.ToString(),
                booking.BookingStatus,
                booking.FlightID.ToString(),
                booking.SeatID,
                booking.SeatClass,
                $"{booking.PassengerFirstName} {booking.PassengerLastName}",
                booking.DepartureTime.ToString("g"),
                booking.ArrivalTime.ToString("g")
            );
        }

        return table;
    }
}