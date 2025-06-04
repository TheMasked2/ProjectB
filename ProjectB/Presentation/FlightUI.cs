using Spectre.Console;

public static class FlightUI
{
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    private static void WaitForKeyPress()
    {
        AnsiConsole.MarkupLine("\n[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }
    
    public static void DisplayAllFlights()
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
            return; // or handle the error as needed
        }
        hasFilters = true;

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

        if (!SessionManager.CurrentUser.IsAdmin)
        {
            seatClass = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]*Seat class (e.g., Economy):[/]")
                    .PromptStyle(highlightStyle));
        }

        var flights = FlightLogic.GetFilteredFlights(origin, destination, startDate, seatClass);

        if (!hasFilters)
        {
            AnsiConsole.MarkupLine("\n[yellow]No filters applied - showing all flights[/]");
        }

        DisplayFilteredFlights(flights);
        WaitForKeyPress();
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

    public static void AddFlight()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Add New Flight[/]").RuleStyle(primaryStyle));

        try
        {
            var flight = new FlightModel();

            // Basic input collection with validation
            while (true)
            {
                try
                {
                    flight.Airline = AnsiConsole.Prompt(
                        new TextPrompt<string>("[#864000]Enter Airline:[/]")
                            .DefaultValue("AIRTREIDES")
                            .PromptStyle(highlightStyle));

                    flight.AirplaneID = AnsiConsole.Prompt(
                        new TextPrompt<string>("[#864000]Enter Aircraft ID:[/]")
                            .PromptStyle(highlightStyle));

                    flight.DepartureAirport = AnsiConsole.Prompt(
                        new TextPrompt<string>("[#864000]Enter Departure Airport:[/]")
                            .PromptStyle(highlightStyle));

                    flight.ArrivalAirport = AnsiConsole.Prompt(
                        new TextPrompt<string>("[#864000]Enter Arrival Airport:[/]")
                            .PromptStyle(highlightStyle));

                    flight.DepartureTime = AnsiConsole.Prompt(
                        new TextPrompt<DateTime>("[#864000]Enter Departure Time (yyyy-MM-dd HH:mm):[/]")
                            .PromptStyle(highlightStyle));

                    flight.ArrivalTime = AnsiConsole.Prompt(
                        new TextPrompt<DateTime>("[#864000]Enter Arrival Time (yyyy-MM-dd HH:mm):[/]")
                            .PromptStyle(highlightStyle));

                    // flight.AvailableSeats = AnsiConsole.Prompt(
                    //     new TextPrompt<int>("[#864000]Enter Available Seats:[/]")
                    //         .PromptStyle(highlightStyle));

                    // flight.Price = AnsiConsole.Prompt(
                    //     new TextPrompt<int>("[#864000]Enter Price:[/]")
                    //         .PromptStyle(highlightStyle));

                    // Validate and add flight
                    if (FlightLogic.AddFlight(flight))
                    {
                        AnsiConsole.MarkupLine("[green]Flight added successfully![/]");
                        WaitForKeyPress();
                        break;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Failed to add flight. Please try again.[/]");
                        WaitForKeyPress();
                    }
                }
                catch (ArgumentException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    WaitForKeyPress();
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]An unexpected error occurred: {ex.Message}[/]");
            WaitForKeyPress();
        }
    }

    public static void EditFlight()
    {
        DisplayAllFlights();

        var flightId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter Flight ID to edit:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => id > 0));

        var flight = FlightLogic.GetFlightById(flightId);
        if (flight == null)
        {
            AnsiConsole.MarkupLine("[red]Flight not found.[/]");
            WaitForKeyPress();
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Edit Flight[/]").RuleStyle(primaryStyle));

        // Same prompts as AddFlight but with DefaultValue set to current values
        flight.Airline = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new Airline[/]")
                .DefaultValue(flight.Airline)
                .PromptStyle(highlightStyle));

        flight.DepartureAirport = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new Departure Airport[/]")
            .DefaultValue(flight.DepartureAirport)
            .PromptStyle(highlightStyle));

        flight.ArrivalAirport = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new Arrival Airport[/]")
            .DefaultValue(flight.ArrivalAirport)
            .PromptStyle(highlightStyle));

        flight.DepartureTime = AnsiConsole.Prompt(
            new TextPrompt<DateTime>("[#864000]Enter new Departure Time (yyyy-MM-dd HH:mm)[/]")
            .DefaultValue(flight.DepartureTime)
            .PromptStyle(highlightStyle));

        flight.ArrivalTime = AnsiConsole.Prompt(
            new TextPrompt<DateTime>("[#864000]Enter new Arrival Time (yyyy-MM-dd HH:mm)[/]")
            .DefaultValue(flight.ArrivalTime)
            .PromptStyle(highlightStyle));

        if (FlightLogic.UpdateFlight(flight))
        {
            AnsiConsole.MarkupLine("[green]Flight updated successfully![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to update flight.[/]");
        }
        WaitForKeyPress();
    }

    public static void RemoveFlight()
    {
        DisplayAllFlights();

        var flightId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter Flight ID to remove:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => id > 0));

        if (AnsiConsole.Confirm("[yellow]Are you sure you want to delete this flight?[/]"))
        {
            if (TryDeleteFlight(flightId))
            {
                AnsiConsole.MarkupLine("[green]Flight deleted successfully![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Failed to delete flight.[/]");
            }
        }
        WaitForKeyPress();
    }

    private static bool TryDeleteFlight(int flightId)
    {
        try
        {
            return FlightLogic.DeleteFlight(flightId);
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[red]Error occurred while deleting the flight.[/]");
            return false;
        }
    }
}