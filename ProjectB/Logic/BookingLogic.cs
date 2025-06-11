using ProjectB.DataAccess;
using Spectre.Console;

public static class BookingLogic
{
    public static IBookingAccess BookingAccessService { get; set; } = new BookingAccess();
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    public static SeatMapLogic SeatMapLogicService { get; set; } = new SeatMapLogic();
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



    public static decimal CalculateBookingPrice(User user, FlightModel flight, SeatModel seat, int amountLuggage, bool isInsurance)
    {
        decimal finalPrice = (decimal)seat.Price;

        // Add luggage cost
        if (amountLuggage > 0)
        {
            finalPrice += 500 * amountLuggage;
        }

        // Apply discounts
        if (user.FirstTimeDiscount)
        {
            finalPrice *= 0.9m; // 10% discount
        }
        else if (DateTime.Now >= user.BirthDate.AddYears(65))
        {
            finalPrice *= 0.8m; // 20% discount
        }

        return finalPrice;
    }

    public static void CreateBooking(User user, FlightModel flight, SeatModel seat, int amountLuggage = 0, bool insuranceStatus = false)
    {
        decimal totalPrice = CalculateBookingPrice(user, flight, seat, amountLuggage, insuranceStatus);

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
            AmountLuggage = amountLuggage,
            InsuranceStatus = insuranceStatus
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

    public static void BookingAFlight(int flightID)
    {
        var flight = FlightLogic.GetFlightById(flightID);
        if (flight == null)
        {
            AnsiConsole.MarkupLine("[red]Flight not found.[/]");
            BookingUI.WaitForKeyPress();
            return;
        }

        // Use flightID for seat map and booking
        var seats = SeatMapLogicService.GetSeatMap(flightID);
        if (seats == null || seats.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No seats found for this airplane.[/]");
            BookingUI.WaitForKeyPress();
            return;
        }

        // Arrange seat letters for known layouts
        var (seatLettersRaw, rowNumbers) = SeatMapLogicService.GetSeatLayout(seats);
        List<string> seatLetters;
        int colCount = seatLettersRaw.Count;
        if (colCount == 2)
            seatLetters = new List<string> { "A", "B" };
        else if (colCount == 4)
            seatLetters = new List<string> { "A", "B", "C", "D" };
        else if (colCount == 6)
            seatLetters = new List<string> { "A", "B", "C", "D", "E", "F" };
        else if (colCount == 8)
            seatLetters = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H" };
        else if (colCount == 10)
            seatLetters = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K" };
        else
            seatLetters = seatLettersRaw; // fallback to whatever is in the DB

        // Determine aisle positions based on column count and arrangement
        List<int> aisleAfter = new();
        if (colCount == 2) aisleAfter.Add(0); // A | B
        else if (colCount == 4) aisleAfter.Add(1); // A B | C D
        else if (colCount == 6) aisleAfter.Add(2); // A B C | D E F
        else if (colCount == 8) { aisleAfter.Add(2); aisleAfter.Add(4); } // A B C | D E | F G H
        else if (colCount == 10) { aisleAfter.Add(2); aisleAfter.Add(6); } // A B C | D E F G | H J K

        AnsiConsole.MarkupLine("[green]Seat Map:[/]");
        var seatArt = new List<string>();

        // Header with aisle separation (4 spaces for each aisle)
        string header = "    ";
        for (int i = 0; i < seatLetters.Count; i++)
        {
            header += $" {seatLetters[i]} ";
            if (aisleAfter.Contains(i))
                header += "    ";
        }
        seatArt.Add(header);

        foreach (var row in rowNumbers)
        {
            string line = $"{row,3} ";
            for (int i = 0; i < seatLetters.Count; i++)
            {
                var letter = seatLetters[i];
                var seat = seats.FirstOrDefault(s => s.RowNumber == row && s.SeatPosition == letter);
                if (seat == null)
                {
                    line += "   ";
                }
                else if (seat.IsOccupied)
                {
                    line += "[red] X [/]";
                }
                else
                {
                    var seatType = (seat.SeatType ?? "").Trim().ToLower();
                    switch (seatType)
                    {
                        case "luxury":
                            line += "[yellow] L [/]";
                            break;
                        case "premium":
                            line += "[magenta] P [/]";
                            break;
                        case "standard extra legroom":
                            line += "[blue] E [/]";
                            break;
                        case "business":
                            line += "[cyan] B [/]";
                            break;
                        case "standard":
                            line += "[green] O [/]";
                            break;
                        default:
                            // Default to Economy if seat type is missing or unknown
                            line += "[grey] ? [/]";
                            break;
                    }
                }
                if (aisleAfter.Contains(i))
                    line += "    ";
            }
            seatArt.Add(line);
        }

        foreach (var l in seatArt)
            AnsiConsole.MarkupLine(l);

        AnsiConsole.MarkupLine(
            "[yellow]L[/]=Luxury  [magenta]P[/]=Premium  [blue]E[/]=Standard Extra Legroom  [cyan]B[/]=Business  [green]O[/]=Standard  [red]X[/]=Occupied"
        );

        // Seat selection input
        SeatModel selectedSeat = null;
        while (selectedSeat == null)
        {
            var seatInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter seat (e.g., 12A or A12):[/]")
                    .PromptStyle(highlightStyle)
            ).Trim();

            string rowPart = "";
            string letterPart = "";

            // Try row+letter (e.g., 12A)
            var rowFirst = new string(seatInput.TakeWhile(char.IsDigit).ToArray());
            var letterAfter = new string(seatInput.SkipWhile(char.IsDigit).ToArray()).ToUpper();

            // Try letter+row (e.g., A12)
            var letterFirst = new string(seatInput.TakeWhile(char.IsLetter).ToArray()).ToUpper();
            var rowAfter = new string(seatInput.SkipWhile(char.IsLetter).ToArray());

            if (!string.IsNullOrEmpty(rowFirst) && !string.IsNullOrEmpty(letterAfter))
            {
                rowPart = rowFirst;
                letterPart = letterAfter;
            }
            else if (!string.IsNullOrEmpty(letterFirst) && !string.IsNullOrEmpty(rowAfter))
            {
                rowPart = rowAfter;
                letterPart = letterFirst;
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Invalid seat format. Please enter a row number and seat letter (e.g., 12A or A12).[/]");
                continue;
            }

            if (int.TryParse(rowPart, out int row))
            {
                selectedSeat = SeatMapLogicService.TryGetAvailableSeat(seats, row, letterPart);
                if (selectedSeat == null)
                {
                    AnsiConsole.MarkupLine("[red]Seat does not exist or is already occupied.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Invalid row number in seat format.[/]");
            }
        }

        // Book the seat for this flight
        SeatMapLogicService.BookSeat(flightID, selectedSeat);

        if (SessionManager.CurrentUser == null)
        {
            int AmountLuggage = PurchaseExtraLuggage();
            bool insuranceStatus = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
            .Title("[#864000]Do you want to purchase travel insurance?[/]")
            .AddChoices(new[] { true, false })
            .UseConverter(choice => choice ? "Yes" : "No")
            .HighlightStyle(highlightStyle)
            );

            decimal finalPrice = (decimal)selectedSeat.Price;
            if (AmountLuggage > 0)
            {
                finalPrice += 500 * AmountLuggage; // FIX PRICE <================================================================================================
            }
            if (insuranceStatus)
            {
                finalPrice += 100000000000; //FIX PRICE <================================================================================================
                AnsiConsole.MarkupLine("[green]Travel insurance purchased![/]");
            }

            AnsiConsole.MarkupLine("[yellow]You are currently logged in as a guest user. Bookings will not be saved to the database.[/]");
            string email = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter your email address for booking confirmation:[/]")
                    .PromptStyle(highlightStyle));

            BookingUI.DisplayBookingDetails(selectedSeat, flight, email, finalPrice);
            SessionManager.Logout(); // Log out guest user after booking
        }
        else // Registered user
        {
            int AmountLuggage = PurchaseExtraLuggage();
            bool insuranceStatus = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
            .Title("[#864000]Do you want to purchase travel insurance?[/]")
            .AddChoices(new[] { true, false })
            .UseConverter(choice => choice ? "Yes" : "No")
            .HighlightStyle(highlightStyle)
            );

            CreateBooking(SessionManager.CurrentUser, flight, selectedSeat, AmountLuggage, insuranceStatus);

            decimal finalPrice = (decimal)selectedSeat.Price;
            if (AmountLuggage > 0)
            {
                finalPrice += 500 * AmountLuggage; //FIX PRICE <================================================================================================
            }
            if (insuranceStatus)
            {
                finalPrice += 100000000000; //FIX PRICE <================================================================================================
                AnsiConsole.MarkupLine("[green]Travel insurance purchased![/]");
            }

            if (SessionManager.CurrentUser.FirstTimeDiscount)
            {
                finalPrice *= 0.75m;
                AnsiConsole.MarkupLine("[green]Congratulations! You have received a 25% discount on your first booking![/]");
                SessionManager.CurrentUser.FirstTimeDiscount = false;
                UserLogic.UpdateUser(SessionManager.CurrentUser);
            }
            else if (DateTime.Now >= SessionManager.CurrentUser.BirthDate.AddYears(65))
            {
                finalPrice *= 0.8m;
                AnsiConsole.MarkupLine("[green]Senior discount (20%) applied![/]");
            }
            BookingUI.DisplayBookingDetails(selectedSeat, flight, null, finalPrice);
        }

        BookingUI.WaitForKeyPress();
    }

    public static void ViewUserBookings(bool upcoming)
    {
        if (!SessionManager.IsLoggedIn())
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to view bookings.[/]");
            BookingUI.WaitForKeyPress();
            return;
        }
        var user = SessionManager.CurrentUser;
        var bookings = BookingLogic.GetBookingsForUser(user.UserID, upcoming);
        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No bookings found.[/]");
            BookingUI.WaitForKeyPress();
            return;
        }
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();
        table.AddColumns("BookingID", "FlightID", "Seat", "Class", "BookingDate", "BoardingTime", "Status", "Payment");
        foreach (var b in bookings)
        {
            table.AddRow(
                b.BookingID.ToString(),
                b.FlightID.ToString(),
                b.SeatID,
                b.SeatClass,
                b.BookingDate,
                b.BoardingTime,
                b.BookingStatus,
                b.PaymentStatus
            );
        }
        AnsiConsole.Write(table);
        BookingUI.WaitForKeyPress();
    }

    private static int PurchaseExtraLuggage()
    {
        var user = SessionManager.CurrentUser;
        bool extraLuggage = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[#864000]Do you want to purchase extra luggage?[/]")
                .AddChoices(new[] { true, false })
                .UseConverter(choice => choice ? "Yes" : "No")
                .HighlightStyle(highlightStyle)
        );
        if (extraLuggage)
        {
            int additionalLuggage = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter the number of additional luggage pieces (1 or 2):[/]")
                .PromptStyle(highlightStyle)
                .Validate(input => input >= 1 && input <= 2, "Please enter a number between 1 or 2.")
            );
            AnsiConsole.MarkupLine($"[green]Successfully purchased {additionalLuggage} extra luggage piece(s)![/]");
            return additionalLuggage;
        }
        return 0;
    }

    public static void CancelBookingPrompt() // DONT INCLUDE CANCELLATION FEE IF THE USER HAS INSURANCE
    {
        if (!SessionManager.IsLoggedIn())
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to cancel a booking.[/]");
            return;
        }

        var user = SessionManager.CurrentUser;
        var bookings = BookingLogic.GetBookingsForUser(user.UserID, true);
        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You have no upcoming bookings to cancel.[/]");
            return;
        }


        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();
        table.AddColumns("BookingID", "FlightID", "Seat", "Class", "BookingDate", "BoardingTime", "Status", "Payment");
        foreach (var b in bookings)
        {
            table.AddRow(
                b.BookingID.ToString(),
                b.FlightID.ToString(),
                b.SeatID,
                b.SeatClass,
                b.BookingDate,
                b.BoardingTime,
                b.BookingStatus,
                b.PaymentStatus
            );
        }
        AnsiConsole.Write(table);


        int bookingId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter the Booking ID to cancel:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => bookings.Any(b => b.BookingID == id), "[red]Invalid Booking ID.[/]")
        );

        var selectedBooking = bookings.First(b => b.BookingID == bookingId);


        if (selectedBooking.BookingStatus?.ToLower() == "cancelled")
        {
            AnsiConsole.MarkupLine("[yellow]This booking is already cancelled.[/]");
            BookingUI.WaitForKeyPress();
            return;
        }

        bool cancelled = BookingLogic.CancelBooking(bookingId);
        if (cancelled)
        {
            AnsiConsole.MarkupLine("[green]Booking successfully cancelled![/]");
            AnsiConsole.MarkupLine("[yellow]A cancellation fee of $100 has been applied.[/]");

            var flight = FlightLogic.GetFlightById(selectedBooking.FlightID);
            var seat = SeatMapLogicService.GetSeatMap(selectedBooking.FlightID)
                .FirstOrDefault(s => s.SeatID == selectedBooking.SeatID);

            if (flight != null && seat != null)
            {
                BookingUI.DisplayBookingDetails(seat, flight, AmountLuggage:selectedBooking.AmountLuggage);
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]BookingID:[/] {selectedBooking.BookingID}");
                AnsiConsole.MarkupLine($"[yellow]FlightID:[/] {selectedBooking.FlightID}");
                AnsiConsole.MarkupLine($"[yellow]Seat:[/] {selectedBooking.SeatID}");
                AnsiConsole.MarkupLine($"[yellow]Class:[/] {selectedBooking.SeatClass}");
                AnsiConsole.MarkupLine($"[yellow]BookingDate:[/] {selectedBooking.BookingDate}");
                AnsiConsole.MarkupLine($"[yellow]BoardingTime:[/] {selectedBooking.BoardingTime}");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Cancellation failed.[/]");
        }
        BookingUI.WaitForKeyPress();
    }
    public static void ModifyBookingPrompt()
    {
        if (!SessionManager.IsLoggedIn())
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to modify a booking.[/]");
            return;
        }

        var user = SessionManager.CurrentUser;
        var bookings = BookingLogic.GetBookingsForUser(user.UserID, true);
        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You have no upcoming bookings to modify.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();
        table.AddColumns("BookingID", "FlightID", "Seat", "Class", "BookingDate", "BoardingTime", "Status", "Payment");
        foreach (var b in bookings)
        {
            table.AddRow(
                b.BookingID.ToString(),
                b.FlightID.ToString(),
                b.SeatID,
                b.SeatClass,
                b.BookingDate,
                b.BoardingTime,
                b.BookingStatus,
                b.PaymentStatus
            );
        }
        AnsiConsole.Write(table);


        int bookingId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter the Booking ID to modify:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => bookings.Any(b => b.BookingID == id), "[red]Invalid Booking ID.[/]")
        );

        var selectedBooking = bookings.First(b => b.BookingID == bookingId);


        if (selectedBooking.BookingStatus?.ToLower() == "cancelled")
        {
            AnsiConsole.MarkupLine("[yellow]This booking is already cancelled and cannot be modified.[/]");
            BookingUI.WaitForKeyPress();
            return;
        }


        var flight = FlightLogic.GetFlightById(selectedBooking.FlightID);
        var seat = SeatMapLogicService.GetSeatMap(selectedBooking.FlightID)
            .FirstOrDefault(s => s.SeatID == selectedBooking.SeatID);

        AnsiConsole.MarkupLine("[yellow]Current booking details:[/]");
        if (flight != null && seat != null)
        {
            BookingUI.DisplayBookingDetails(seat, flight, AmountLuggage:selectedBooking.AmountLuggage);
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]BookingID:[/] {selectedBooking.BookingID}");
            AnsiConsole.MarkupLine($"[yellow]FlightID:[/] {selectedBooking.FlightID}");
            AnsiConsole.MarkupLine($"[yellow]Seat:[/] {selectedBooking.SeatID}");
            AnsiConsole.MarkupLine($"[yellow]Class:[/] {selectedBooking.SeatClass}");
            AnsiConsole.MarkupLine($"[yellow]BookingDate:[/] {selectedBooking.BookingDate}");
            AnsiConsole.MarkupLine($"[yellow]BoardingTime:[/] {selectedBooking.BoardingTime}");
        }

        var seats = SeatMapLogicService.GetSeatMap(selectedBooking.FlightID);
        if (seats == null || seats.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No seats found for this flight.[/]");
            BookingUI.WaitForKeyPress();
            return;
        }

        string newSeatId = null;
        while (true)
        {
            AnsiConsole.MarkupLine("[yellow]Enter the new seat (e.g., 12A or A12):[/]");
            var seatInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Seat:[/]").PromptStyle(highlightStyle)
            ).Trim().ToUpper();

            string rowPart = "";
            string letterPart = "";

            // Try row+letter (e.g., 12A)
            var rowFirst = new string(seatInput.TakeWhile(char.IsDigit).ToArray());
            var letterAfter = new string(seatInput.SkipWhile(char.IsDigit).ToArray()).ToUpper();

            // Try letter+row (e.g., A12)
            var letterFirst = new string(seatInput.TakeWhile(char.IsLetter).ToArray()).ToUpper();
            var rowAfter = new string(seatInput.SkipWhile(char.IsLetter).ToArray());

            if (!string.IsNullOrEmpty(rowFirst) && !string.IsNullOrEmpty(letterAfter))
            {
                rowPart = rowFirst;
                letterPart = letterAfter;
            }
            else if (!string.IsNullOrEmpty(letterFirst) && !string.IsNullOrEmpty(rowAfter))
            {
                rowPart = rowAfter;
                letterPart = letterFirst;
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Invalid seat format. Please enter a row number and seat letter (e.g., 12A or A12).[/]");
                continue;
            }

            if (int.TryParse(rowPart, out int row))
            {
                var seatObj = seats.FirstOrDefault(s =>
                    s.RowNumber == row &&
                    s.SeatPosition.Equals(letterPart, StringComparison.OrdinalIgnoreCase)
                );

                if (seatObj == null)
                {
                    AnsiConsole.MarkupLine("[red]Seat does not exist.[/]");
                    continue;
                }
                if (seatObj.IsOccupied && seatObj.SeatID != selectedBooking.SeatID)
                {
                    AnsiConsole.MarkupLine("[red]Seat is already occupied.[/]");
                    continue;
                }
                newSeatId = seatObj.SeatID;
                break;
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Invalid row number in seat format.[/]");
            }
        }

        int newLuggage = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter new luggage amount (0-2):[/]")
                .PromptStyle(highlightStyle)
                .Validate(l => l >= 0 && l <= 2, "[red]Luggage must be 0, 1, or 2.[/]")
        );

        if (newLuggage > 0)
        {
            AnsiConsole.MarkupLine($"[yellow]A surcharge of ${newLuggage * 50} has been added for extra luggage.[/]");
        }

        BookingLogic.ModifyBooking(bookingId, newSeatId, newLuggage);
        BookingUI.WaitForKeyPress();
    }
}
