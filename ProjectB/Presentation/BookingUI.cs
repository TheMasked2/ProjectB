using Spectre.Console;

public static class BookingUI
{
    public static SeatMapLogic SeatMapLogicService { get; set; } = new SeatMapLogic();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    private static void WaitForKeyPress()
    {
        AnsiConsole.MarkupLine("\n[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }

    public static void DisplayAllBookableFlights()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Flight Search")
                    .Centered()
                    .Color(Color.Orange1));

            bool hasFilters = false;
            AnsiConsole.MarkupLine("\n[#864000]Enter filter criteria (fields that start with * are mandatory!):[/]");

            string origin = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]*Origin airport (e.g., LAX):[/]")
                    .PromptStyle(highlightStyle));
            hasFilters |= !string.IsNullOrWhiteSpace(origin);

            string destination = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]*Destination airport (e.g., JFK):[/]")
                    .PromptStyle(highlightStyle));
            hasFilters |= !string.IsNullOrWhiteSpace(destination);

            string startDateInput = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]*Start date (yyyy-MM-dd):[/]")
                .PromptStyle(highlightStyle));

            DateTime startDate;
            if (!DateTime.TryParse(startDateInput, out startDate))
            {
                AnsiConsole.MarkupLine("[red]Invalid start date format. Please use yyyy-MM-dd.[/]");
                WaitForKeyPress();
                continue;
            }
            hasFilters = true;

            string endDateInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]*End date (yyyy-MM-dd):[/]")
                    .PromptStyle(highlightStyle));

            DateTime endDate;
            if (!DateTime.TryParse(endDateInput, out endDate))
            {
                AnsiConsole.MarkupLine("[red]Invalid end date format. Please use yyyy-MM-dd.[/]");
                WaitForKeyPress();
                continue;
            }
            hasFilters = true;

            string minPriceInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Minimum price:[/]")
                    .PromptStyle(highlightStyle)
                    .AllowEmpty());
            int? minPrice = !string.IsNullOrWhiteSpace(minPriceInput) && int.TryParse(minPriceInput, out int mp) ? mp : null;
            hasFilters |= minPrice.HasValue;

            string maxPriceInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Maximum price:[/]")
                    .PromptStyle(highlightStyle)
                    .AllowEmpty());
            int? maxPrice = !string.IsNullOrWhiteSpace(maxPriceInput) && int.TryParse(maxPriceInput, out int mxp) ? mxp : null;
            hasFilters |= maxPrice.HasValue;

            string seatClass = null;
            // if (!SessionManager.CurrentUser.IsAdmin)
            // {
            //     seatClass = AnsiConsole.Prompt(
            //         new TextPrompt<string>("[#864000]*Seat class (e.g., Economy):[/]")
            //             .PromptStyle(highlightStyle));
            // }

            var flights = FlightLogic.GetFilteredFlights(origin, destination, startDate, endDate, minPrice, maxPrice, seatClass);

            if (!hasFilters)
            {
                AnsiConsole.MarkupLine("\n[yellow]No filters applied - showing all flights[/]");
            }

            if (flights == null || !flights.Any())
            {
                var panel = new Panel("[yellow]No flights found matching the criteria. Please try again.[/]\n[grey]Press [bold]Escape[/] to return to the main menu, or any other key to try again.[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(errorStyle);
                AnsiConsole.Write(panel);

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                    break; // Return to main menu

                continue; // Prompt again for filter criteria
            }

            DisplayFilteredFlights(flights);

            AnsiConsole.MarkupLine("\n[green]Select a flight to book:[/]");
            int flightIdInput = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]Flight ID:[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(flightIdInput => flightIdInput > 0));

            BookingAFlight(flightIdInput);
            break;
        }
    }

    private static void DisplayFilteredFlights(List<FlightModel> flights)
    {
        if (flights == null || !flights.Any())
        {
            var panel = new Panel("[yellow]No flights found matching the criteria.[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(errorStyle);
            AnsiConsole.Write(panel);
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();

        table.AddColumns(
            "[#864000]ID[/]", "[#864000]Aircraft ID[/]", "[#864000]Airline[/]",
            "[#864000]From[/]", "[#864000]To[/]", "[#864000]Departure[/]",
            "[#864000]Arrival[/]", "[#864000]Price[/]", "[#864000]Status[/]"
        );

        foreach (var flight in flights)
        {
            table.AddRow(
                flight.FlightID.ToString(),
                flight.AirplaneID,
                flight.Airline,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureTime.ToString("g"),
                flight.ArrivalTime.ToString("g"),
                $"€{flight.Price:F2}",
                flight.FlightStatus
            );
        }

        AnsiConsole.Write(table);
    }

    private static void BookingAFlight(int flightID)
    {
        var flight = FlightLogic.GetFlightById(flightID);
        if (flight == null)
        {
            AnsiConsole.MarkupLine("[red]Flight not found.[/]");
            WaitForKeyPress();
            return;
        }

        // Use flightID for seat map and booking
        var seats = SeatMapLogicService.GetSeatMap(flightID);
        if (seats == null || seats.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No seats found for this airplane.[/]");
            WaitForKeyPress();
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


        // Only create a booking if the user is not a guest
        // if (SessionManager.CurrentUser != null && !SessionManager.CurrentUser.Guest)
        // {
        //     BookingLogic.CreateBooking(SessionManager.CurrentUser, flight, selectedSeat);
        //     AnsiConsole.MarkupLine("[green]Booking successful![/]");
        //     AnsiConsole.MarkupLine($"[yellow]Seat[/]: [white]{selectedSeat.RowNumber}{selectedSeat.SeatPosition}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]Seat Type[/]: [white]{selectedSeat.SeatType ?? "-"}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]Price[/]: [white]€{selectedSeat.Price:F2}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]Airplane ID[/]: [white]{selectedSeat.AirplaneID}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]Flight ID[/]: [white]{flight.FlightID}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]From[/]: [white]{flight.DepartureAirport}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]To[/]: [white]{flight.ArrivalAirport}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]Departure[/]: [white]{flight.DepartureTime:g}[/]");
        //     AnsiConsole.MarkupLine($"[yellow]Arrival[/]: [white]{flight.ArrivalTime:g}[/]");
        // }
        if (SessionManager.CurrentUser == null)
        {
            AnsiConsole.MarkupLine("[yellow]You are currently logged in as a guest user. Bookings will not be saved.[/]");
            int AmountLuggage = PurchaseExtraLuggage();
            string email = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter your email address for booking confirmation:[/]")
                    .PromptStyle(highlightStyle)
            );
            AnsiConsole.MarkupLine("[green]Booking successful![/]");
            DisplayBookingDetails(selectedSeat, flight, AmountLuggage, email);
            SessionManager.Logout(); // Log out guest user after booking
    }
        else // Registered user
        {
            int AmountLuggage = PurchaseExtraLuggage();

            BookingLogic.CreateBooking(SessionManager.CurrentUser, flight, selectedSeat, AmountLuggage);
            AnsiConsole.MarkupLine("[green]Booking successful![/]");

            // Calculate price and apply discounts
            decimal finalPrice = (decimal)selectedSeat.Price;
            
            if (AmountLuggage > 0)
            {
                finalPrice += 500 * AmountLuggage;
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


            DisplayBookingDetails(selectedSeat, flight, AmountLuggage, null, finalPrice);
        }

    WaitForKeyPress();
    }

    private static void DisplayBookingDetails(SeatModel seat, FlightModel flight, int AmountLuggage, string email = null, decimal? overridePrice = null)
    {
        if (email != null)
        {
            AnsiConsole.MarkupLine($"[green]Booking confirmation will be sent to: {email}[/]");
        }
        if (AmountLuggage > 0)
        {
            overridePrice += 500 * AmountLuggage;
        }

        AnsiConsole.MarkupLine($"[yellow]Seat[/]: [white]{seat.RowNumber}{seat.SeatPosition}[/]");
        AnsiConsole.MarkupLine($"[yellow]Seat Type[/]: [white]{seat.SeatType ?? "-"}[/]");
        AnsiConsole.MarkupLine($"[yellow]Price[/]: [white]€{overridePrice ?? (decimal)seat.Price:F2}[/]");
        AnsiConsole.MarkupLine($"[yellow]Airplane ID[/]: [white]{seat.AirplaneID}[/]");
        AnsiConsole.MarkupLine($"[yellow]Flight ID[/]: [white]{flight.FlightID}[/]");
        AnsiConsole.MarkupLine($"[yellow]From[/]: [white]{flight.DepartureAirport}[/]");
        AnsiConsole.MarkupLine($"[yellow]To[/]: [white]{flight.ArrivalAirport}[/]");
        AnsiConsole.MarkupLine($"[yellow]Departure[/]: [white]{flight.DepartureTime:g}[/]");
        AnsiConsole.MarkupLine($"[yellow]Arrival[/]: [white]{flight.ArrivalTime:g}[/]");
        AnsiConsole.MarkupLine($"[yellow]Luggage[/]: [white]{AmountLuggage:g}[/]");
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

    public static void ViewUserBookings(bool upcoming)
    {
        if (!SessionManager.IsLoggedIn())
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to view bookings.[/]");
            WaitForKeyPress();
            return;
        }
        var user = SessionManager.CurrentUser;
        var bookings = BookingLogic.GetBookingsForUser(user.UserID, upcoming);
        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No bookings found.[/]");
            WaitForKeyPress();
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
        WaitForKeyPress();
    }
}
