using Spectre.Console;

public static class BookingUI
{
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static void WaitForKeyPress()
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

            AnsiConsole.MarkupLine("\n[#864000]Enter filter criteria (fields that start with * are mandatory!):[/]");

            string origin = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]*Origin airport (e.g., LAX):[/]")
                    .PromptStyle(highlightStyle));

            string destination = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]*Destination airport (e.g., JFK):[/]")
                    .PromptStyle(highlightStyle));

            string departureDateInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]*Departure date (yyyy-MM-dd):[/]")
                .DefaultValue(DateTime.Now.ToString("yyyy-MM-dd"))
                    .PromptStyle(highlightStyle));
            DateTime departureDate;
            if (!DateTime.TryParse(departureDateInput, out departureDate))
            {
                AnsiConsole.MarkupLine("[red]Invalid date format. Please use yyyy-MM-dd.[/]");
                WaitForKeyPress();
                continue;
            }

            var seatClassOptions = new List<string>
            {
                "Luxury",
                "Business",
                "Premium",
                "Standard Extra Legroom",
                "Standard"
            };

            var input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#864000]Select seat class (default is 'Standard'):[/]")
                    .PageSize(6)
                    .AddChoices(seatClassOptions));

            string seatClass;
            switch (input)
            {
                case "Standard":
                    seatClass = "Standard";
                    break;
                case "Business":
                    seatClass = "Business";
                    break;
                case "Premium":
                    seatClass = "Premium";
                    break;
                case "Luxury":
                    seatClass = "Luxury";
                    break;
                case "Standard Extra Legroom":
                    seatClass = "Standard Extra Legroom";
                    break;
                default:
                    seatClass = "Standard"; // Any
                    break;
            }

            var flights = FlightLogic.GetFilteredFlights(origin, destination, departureDate);

            if (flights == null || flights.Count == 0)
            {
                var panel = new Panel("[yellow]No flights found matching the criteria. Please try again.[/]\n[grey]Press [bold]Escape[/] to return to the main menu, or any other key to try again.[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(errorStyle);
                AnsiConsole.Write(panel);

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                    break;

                continue;
            }

            // Do NOT filter out flights based on price/seat class availability
            AnsiConsole.Write(FlightLogic.DisplayFilteredFlights(flights, seatClass));

            if (!flights.Any())
            {
                WaitForKeyPress();
                break;
            }

            AnsiConsole.MarkupLine("\n[green]Select a flight to book:[/]");
            int flightIdInput = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]Flight ID:[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(flightIdInput => flightIdInput > 0));

            BookingLogic.BookingAFlight(flightIdInput);
            break;
        }
    }   

    public static void DisplayBookingDetails(SeatModel seat, FlightModel flight, string email = null, decimal? overridePrice = null)
    {
        if (email != null)
        {
            AnsiConsole.MarkupLine($"[green]Booking confirmation will be sent to: {email}[/]");
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
    }
}
