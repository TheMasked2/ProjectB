using ProjectB.DataAccess;
using Spectre.Console;

public static class BookingLogic
{
    public static IBookingAccess BookingAccessService { get; set; } = new BookingAccess();
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static void BackfillFlightSeats()
    {
        var flights = FlightAccessService.GetAllFlightData();
        var toBackfill = new List<(int, string)>();
        foreach (var flight in flights)
        {
            if (!FlightSeatAccessService.HasAnySeatsForFlight(flight.FlightID))
            {
                Console.WriteLine($"Backfilling seats for FlightID={flight.FlightID}, AirplaneID={flight.AirplaneID}");
                toBackfill.Add((flight.FlightID, flight.AirplaneID));
            }
            else
            {
                Console.WriteLine($"FlightID={flight.FlightID} already has seats.");
            }
        }
        if (toBackfill.Count > 0)
        {
            FlightSeatAccessService.BulkCreateAllFlightSeats(toBackfill);
        }
    }

    public static void CreateBooking(User user, FlightModel flight, SeatModel seat, int amountLuggage)
    {
        var booking = new BookingModel
        {
            UserID = user.UserID,
            PassengerName = $"{user.FirstName} {user.LastName}",
            FlightID = flight.FlightID,
            BookingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            BoardingTime = flight.DepartureTime.ToString("yyyy-MM-dd HH:mm:ss"),
            SeatID = seat.SeatID,
            SeatClass = seat.SeatType,
            BookingStatus = "Confirmed",
            PaymentStatus = "Paid",
            AmountLuggage = amountLuggage
        };
        BookingAccessService.AddBooking(booking);
    }

    public static List<BookingModel> GetBookingsForUser(int userId, bool upcoming)
    {
        var all = BookingAccessService.GetBookingsByUser(userId);
        var now = DateTime.Now;
        return all.Where(b =>
        {
            DateTime flightDate = DateTime.Parse(b.BoardingTime);
            return upcoming ? flightDate >= now : flightDate < now;
        }).ToList();
    }

    public static int GetNextBookingId()
    {
        var all = BookingAccessService.GetBookingsByUser(0);
        return all.Any() ? all.Max(b => b.BookingID) + 1 : 1;
    }


    public static bool CancelBooking(int bookingId)
    {
        var booking = BookingAccessService.GetBookingById(bookingId);
        if (booking == null)
        {
            AnsiConsole.MarkupLine($"[red]Booking with ID {bookingId} not found.[/]");
            return false;
        }

        booking.BookingStatus = "Cancelled";
        BookingAccessService.UpdateBooking(booking);

        // Free up the seat
        var flight = FlightAccessService.GetById(booking.FlightID);
        if (flight != null)
        {
            FlightSeatAccessService.SetSeatOccupied(booking.FlightID, booking.SeatID, false);
        }

        AnsiConsole.MarkupLine($"[green]Booking with ID {bookingId} has been cancelled.[/]");
        return true;
    }
    public static bool ModifyBooking(int bookingId, string newSeatId, int newLuggageAmount)
    {
        var booking = BookingAccessService.GetBookingById(bookingId);
        if (booking == null)
        {
            AnsiConsole.MarkupLine($"[red]Booking with ID {bookingId} not found.[/]");
            return false;
        }

        FlightSeatAccessService.SetSeatOccupied(booking.FlightID, booking.SeatID, false);

        FlightSeatAccessService.SetSeatOccupied(booking.FlightID, newSeatId, true);

        booking.SeatID = newSeatId;
        booking.AmountLuggage = newLuggageAmount;
        BookingAccessService.UpdateBooking(booking);

        AnsiConsole.MarkupLine($"[green]Booking with ID {bookingId} has been updated.[/]");
        return true;
    }
}