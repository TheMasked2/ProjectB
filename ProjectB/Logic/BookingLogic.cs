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
    private static readonly Style errorStyle = new(new Color(162, 52, 0));

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

    public static (decimal finalPrice, decimal discount, decimal insurancePrice) CalculateBookingPrice(
        User user,
        FlightModel flight,
        SeatModel seat,
        int amountLuggage,
        bool isInsurance,
        (bool, bool) coupon)
    {
        decimal finalPrice = (decimal)seat.Price;
        decimal totalDiscount = 1.0m;
        decimal insurancePrice = 0.0m;

        if (isInsurance)
        {
            insurancePrice = (decimal)seat.Price * 0.2m; // 20% of seat price for insurance
            finalPrice += insurancePrice;
        }

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
            return (-1 * finalPrice * totalDiscount, totalDiscount, insurancePrice); // is minus, pay with spice (easter egg)
        }
    

        return (finalPrice * totalDiscount, totalDiscount, insurancePrice);
    }

    public static BookingModel BookingBuilder(User user, FlightModel flight, SeatModel seat, (bool, bool) coupon, int amountLuggage = 0, bool insuranceStatus = false)
    {
        (decimal finalPrice, decimal discount, decimal insurancePrice) calculatedPrice = CalculateBookingPrice(user, flight, seat, amountLuggage, insuranceStatus, coupon);

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

    public static (bool success, bool freeCancel) CancelBooking(int bookingId)
    {
        // Retrieve the booking
        BookingModel booking = BookingAccessService.GetBookingById(bookingId);
        if (booking == null)
        {
            return (false, false);
        }

        // Free up the seat
        FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, booking.SeatID, false);
        
        bool freeCancel = booking.HasInsurance;
        
        // Update the booking based on insurance status
        if (booking.HasInsurance)
        {
            booking.TotalPrice = 0; // Full refund with insurance
        }
        else
        {
            booking.TotalPrice = Math.Max(0, booking.TotalPrice - 100); // Apply $100 cancellation fee
        }
        
        // Set booking status to cancelled
        booking.BookingStatus = "Cancelled";
        
        // Save the changes
        BookingAccessService.UpdateBooking(booking);
        
        return (true, freeCancel);
    }

    public static bool ModifyBooking(int bookingId, SeatModel newSeatId, int newLuggageAmount)
    {
        BookingModel booking = BookingAccessService.GetBookingById(bookingId);
        if (booking == null)
        {
            return false;
        }

        FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, booking.SeatID, false);
        FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, newSeatId.SeatType, true);

        (decimal totalprice, decimal discount, decimal insuranceprice) = CalculateBookingPrice(SessionManager.CurrentUser, 
            FlightAccessService.GetById(booking.FlightID), 
            newSeatId, 
            newLuggageAmount, 
            booking.HasInsurance, 
            (false, false));
        booking.TotalPrice = totalprice;
        booking.SeatID = newSeatId.SeatID;
        booking.LuggageAmount = newLuggageAmount;
        booking.SeatClass = newSeatId.SeatType;
        BookingAccessService.UpdateBooking(booking);

        return true;
    }

    public static BookingModel GetBookingById(int bookingId)
    {
        return BookingAccessService.GetBookingById(bookingId);
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