using ProjectB.DataAccess;
using Spectre.Console;

public static class BookingLogic
{
    public static IBookingAccess BookingAccessService { get; set; } = new BookingAccess();
    public static IUserAccess UserAccessService { get; set; } = new UserAccess();
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));

    public static void BackfillFlightSeats(int FlightID)
    {
        FlightModel? flight = FlightAccessService.GetById(FlightID);
        AirplaneModel? airplane = AirplaneLogic.GetAirplaneByID(flight.AirplaneID);

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
        Coupons? coupon)
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
        if (user.IsCustomer && user.FirstTimeDiscount)
        {
            totalDiscount -= 0.1m; // 10% discount
        }

        if (user.IsCustomer && DateTime.Now >= user.BirthDate.AddYears(65))
        {
            totalDiscount -= 0.2m; // 20% discount
        }

        if (coupon.HasValue)
        {
            if (coupon.Value == Coupons.Spice)
            {
                // Easter egg, will pay in spice.
                return (-(finalPrice * totalDiscount), totalDiscount, insurancePrice);
            }
            else
            {
                totalDiscount -= (decimal)coupon.Value / 100.0m;
            }
        }

        return (finalPrice * totalDiscount, totalDiscount, insurancePrice);
    }

    public static BookingModel BookingBuilder(User user, FlightModel flight, SeatModel seat, Coupons? coupon, int amountLuggage = 0, bool insuranceStatus = false)
    {
        (decimal finalPrice, decimal discount, decimal insurancePrice) calculatedPrice = CalculateBookingPrice(user, flight, seat, amountLuggage, insuranceStatus, coupon);

        var booking = new BookingModel
        {
            BookingStatus = "Pending",
            UserID = user.UserID,
            FlightID = flight.FlightID,
            SeatID = seat.SeatID,
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
            
            FlightModel? flight = FlightAccessService.GetById(b.FlightID);
            if (flight == null) return false;

            return upcoming ? flight.DepartureTime >= now : flight.DepartureTime < now;
        }).ToList();
    }

    public static (bool success, bool freeCancel) CancelBooking(int bookingId)
    {
        // Retrieve the booking
        BookingModel? booking = BookingAccessService.GetById(bookingId);
        if (booking is null)
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
        BookingAccessService.Update(booking);
        
        return (true, freeCancel);
    }

    public static bool ModifyBooking(int bookingId, SeatModel newSeatId, int newLuggageAmount)
    {
        BookingModel? booking = BookingAccessService.GetById(bookingId);
        if (booking is null)
        {
            return false;
        }

        FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, booking.SeatID, false);
        FlightSeatAccessService.SetSeatOccupancy(booking.FlightID, newSeatId.SeatID, true);

        (decimal totalprice, decimal discount, decimal insuranceprice) = CalculateBookingPrice(
            SessionManager.CurrentUser,
            FlightAccessService.GetById(booking.FlightID),
            newSeatId,
            newLuggageAmount,
            booking.HasInsurance,
            null
        );
        booking.TotalPrice = totalprice;
        booking.SeatID = newSeatId.SeatID;
        booking.LuggageAmount = newLuggageAmount;

        BookingAccessService.Update(booking);
        return true;
    }

    public static BookingModel? GetBookingById(int bookingId)
    {
        return BookingAccessService.GetById(bookingId);
    }
    public static void BookTheDamnFlight(BookingModel booking)
    {
        booking.BookingStatus = "Confirmed";
        BookingAccessService.Insert(booking);
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

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();
        table.AddColumns("BookingID", "Status", "FlightID", "Seat", "Class", "Passenger", "Departure", "Arrival");

        foreach (BookingModel booking in bookings)
        {
            FlightModel? flight = FlightAccessService.GetById(booking.FlightID);
            User? user = SessionManager.CurrentUser;
            SeatModel? seat = FlightSeatAccessService.GetById(booking.SeatID);
            table.AddRow(
                booking.BookingID.ToString(),
                booking.BookingStatus,
                booking.FlightID.ToString(),
                booking.SeatID,
                seat.SeatClass,
                $"{user.FirstName} {user.LastName}",
                flight.DepartureTime.ToString("g"),
                flight.ArrivalTime.ToString("g")
            );
        }

        return table;
    }
}