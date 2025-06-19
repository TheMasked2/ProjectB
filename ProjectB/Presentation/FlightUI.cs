using Microsoft.VisualBasic;
using Spectre.Console;

public static class FlightUI
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

    public static List<FlightModel> DisplayFilteredUpcomingFlights()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("Flight Search")
                .Centered()
                .Color(Color.Orange1));

        List<AirportModel> airports = AirportLogic.GetAllAirports();
        Table airportTable = AirportLogic.CreateAirportsTable(airports);
        AnsiConsole.Write(airportTable);

        List<string> validIataCodes = airports.Select(airport => airport.IataCode).ToList();

        AnsiConsole.MarkupLine("\n[#864000]Enter filter criteria:[/]");

        string origin = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter origin airport code (IATA):[/]")
                .PromptStyle(highlightStyle)
                .Validate(code =>
                    validIataCodes.Contains(code.ToUpper()),
                    "[red]Invalid airport code. Please use a valid IATA code from the table above.[/]")
        ).ToUpper().Trim();

        string destination = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter destination airport code (IATA):[/]")
                .PromptStyle(highlightStyle)
                .Validate(code =>
                    validIataCodes.Contains(code.ToUpper()) && code.ToUpper() != origin,
                    "[red]Invalid airport code or same as origin. Please use a different valid IATA code from the table above.[/]")
        ).ToUpper().Trim();

        string departureDateInput;
        DateTime departureDate;
        DateTime today = DateTime.Today;
        bool validDate = false;

        do
        {
            departureDateInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Departure date (yyyy-MM-dd). Press Enter to enter current date:[/]")
                .DefaultValue(DateTime.Now.ToString("yyyy-MM-dd"))
                .PromptStyle(highlightStyle));

            if (DateTime.TryParse(departureDateInput, out departureDate))
            {
                validDate = true;
                if (departureDate < today)
                {
                    AnsiConsole.MarkupLine("[red]Departure date cannot be in the past. Please enter a valid date.[/]");
                    validDate = false;
                }
                else
                {
                    departureDate = departureDate.Date; // Normalize to date only
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Invalid date format. Please use yyyy-MM-dd.[/]");
            }
        } while (!validDate);

        List<FlightModel> flights = FlightLogic.GetFilteredFlights(origin, destination, departureDate);

        AnsiConsole.Write(FlightLogic.CreateDisplayableFlightsTable(flights));

        return flights;
    }

    public static List<FlightModel> DisplayAllBookableFlights()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Flight Search")
                    .Centered()
                    .Color(Color.Orange1));


            List<AirportModel> airports = AirportLogic.GetAllAirports();
            Table airportTable = AirportLogic.CreateAirportsTable(airports);
            AnsiConsole.Write(airportTable);

            List<string> validIataCodes = airports.Select(airport => airport.IataCode).ToList();

            AnsiConsole.MarkupLine("\n[#864000]Enter filter criteria:[/]");

            string origin = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter origin airport code (IATA):[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(code =>
                        validIataCodes.Contains(code.ToUpper().Trim()),
                        "[red]Invalid airport code. Please use a valid IATA code from the table above.[/]")
            ).ToUpper();

            string destination = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter destination airport code (IATA):[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(code =>
                        validIataCodes.Contains(code.ToUpper().Trim()) && code.ToUpper().Trim() != origin,
                        "[red]Invalid airport code or same as origin. Please use a different valid IATA code from the table above.[/]")
            ).ToUpper().Trim();

            string departureDateInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Departure date (yyyy-MM-dd). Press Enter to enter current date:[/]")
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

            List<FlightModel> flights = FlightLogic.GetFilteredFlights(origin, destination, departureDate, seatClass);

            AnsiConsole.Write(FlightLogic.CreateDisplayableFlightsTable(flights, seatClass));

            if (!flights.Any())
            {
                WaitForKeyPress();
                break;
            }
            return flights;
        }
        return null;
    }


    public static void AddFlight()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Add New Flight[/]").RuleStyle(primaryStyle));

        FlightModel flight = new FlightModel();

        while (true)
        {
            try
            {
                flight.Airline = AnsiConsole.Prompt(
                    new TextPrompt<string>("[#864000]Enter Airline:[/]")
                        .DefaultValue("AIRTREIDES")
                        .PromptStyle(highlightStyle));

                flight.AirplaneID = SelectAirplaneIDFromList();
                AnsiConsole.MarkupLine($"[green]Selected Airplane ID: {flight.AirplaneID}[/]");

                List<AirportModel> airports = AirportLogic.GetAllAirports();
                Table airportTable = AirportLogic.CreateAirportsTable(airports);
                AnsiConsole.Write(airportTable);

                List<string> validIataCodes = airports.Select(airport => airport.IataCode).ToList();

                string origin = AnsiConsole.Prompt(
                    new TextPrompt<string>("[#864000]Enter origin airport code (IATA):[/]")
                        .PromptStyle(highlightStyle)
                        .Validate(code =>
                            validIataCodes.Contains(code.ToUpper()),
                            "[red]Invalid airport code. Please use a valid IATA code from the table above.[/]")
                ).ToUpper().Trim();

                string destination = AnsiConsole.Prompt(
                    new TextPrompt<string>("[#864000]Enter destination airport code (IATA):[/]")
                        .PromptStyle(highlightStyle)
                        .Validate(code =>
                            validIataCodes.Contains(code.ToUpper().Trim()) && code.ToUpper().Trim() != origin,
                            "[red]Invalid airport code or same as origin. Please use a different valid IATA code from the table above.[/]")
                ).ToUpper().Trim();

                flight.DepartureTime = AnsiConsole.Prompt(
                    new TextPrompt<DateTime>("[#864000]Enter Departure Time (yyyy-MM-dd HH:mm):[/]")
                        .PromptStyle(highlightStyle));

                flight.ArrivalTime = AnsiConsole.Prompt(
                    new TextPrompt<DateTime>("[#864000]Enter Arrival Time (yyyy-MM-dd HH:mm):[/]")
                        .PromptStyle(highlightStyle));

                FlightLogic.AddFlight(flight);
                AnsiConsole.MarkupLine("[green]Flight added successfully![/]");
                WaitForKeyPress();
                break;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                WaitForKeyPress();
            }
        }
    }
    // BUG: If there is no flight with given airports, will be stuck in select flightid prompt
    public static void EditFlight()
    {
        List<FlightModel> flights = DisplayFilteredUpcomingFlights();

        int flightId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter Flight ID to edit flight:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id =>
                {
                    if (flights.Any(f => f.FlightID == id))
                        return true;
                    return false;
                }, "[red]Invalid Flight ID. Please enter an ID from the table above.[/]")
        );

        FlightModel flight = FlightLogic.GetFlightById(flightId);
        if (flight == null)
        {
            AnsiConsole.MarkupLine("[red]Flight not found.[/]");
            WaitForKeyPress();
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Edit Flight[/]").RuleStyle(primaryStyle));

        try
        {
            flight.Airline = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter new Airline[/]")
                    .DefaultValue(flight.Airline)
                    .PromptStyle(highlightStyle));

            List<AirportModel> airports = AirportLogic.GetAllAirports();
            Table airportTable = AirportLogic.CreateAirportsTable(airports);
            AnsiConsole.Write(airportTable);

            List<string> validIataCodes = airports.Select(airport => airport.IataCode).ToList();

            string origin = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter origin airport code (IATA):[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(code =>
                        validIataCodes.Contains(code.ToUpper().Trim()),
                        "[red]Invalid airport code. Please use a valid IATA code from the table above.[/]")
            ).ToUpper().Trim();

            string destination = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter destination airport code (IATA):[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(code =>
                        validIataCodes.Contains(code.ToUpper().Trim()) && code.ToUpper().Trim() != origin,
                        "[red]Invalid airport code or same as origin. Please use a different valid IATA code from the table above.[/]")
            ).ToUpper().Trim();

            flight.DepartureTime = AnsiConsole.Prompt(
                new TextPrompt<DateTime>("[#864000]Enter new Departure Time (yyyy-MM-dd HH:mm)[/]")
                .DefaultValue(flight.DepartureTime)
                .PromptStyle(highlightStyle));

            flight.ArrivalTime = AnsiConsole.Prompt(
                new TextPrompt<DateTime>("[#864000]Enter new Arrival Time (yyyy-MM-dd HH:mm)[/]")
                .DefaultValue(flight.ArrivalTime)
                .PromptStyle(highlightStyle));

            FlightLogic.UpdateFlight(flight);
            AnsiConsole.MarkupLine("[green]Flight updated successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to update flight: {ex.Message}[/]");
        }
        WaitForKeyPress();
    }

    public static void RemoveFlight()
    {
        while (true)
        {
            List<FlightModel> flights = DisplayFilteredUpcomingFlights();
            
            if (!flights.Any())
            {
                AnsiConsole.MarkupLine("[red]No flights available to remove.[/]");
                WaitForKeyPress();
                return;
            }

            var flightId = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]Enter Flight ID to remove:[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(id => 
                    {
                        if (flights.Any(f => f.FlightID == id))
                            return true;
                        return false;
                    }, "[red]Invalid Flight ID. Please enter an ID from the table above.[/]"));

            if (AnsiConsole.Confirm("[yellow]Are you sure you want to delete this flight?[/]"))
            {
                try
                {
                    FlightLogic.DeleteFlight(flightId);
                    AnsiConsole.MarkupLine("[green]Flight successfully deleted.[/]");
                    WaitForKeyPress();
                    return;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to delete flight: {ex.Message}[/]");
                    WaitForKeyPress();
                    return;
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Flight deletion cancelled.[/]");
                AnsiConsole.MarkupLine("[grey]Press any key to return to main menu.[/]");
                Console.ReadKey(true);
                return;
            }
        }
    }

    public static void SearchFlights()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("Flight Search")
                .Centered()
                .Color(Color.Orange1));
        int FlightID = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter Flight ID to search:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => id > 0));

        FlightModel flight = FlightLogic.GetFlightById(FlightID);
        if (flight == null)
        {
            AnsiConsole.MarkupLine("[red]Flight not found.[/]");
            WaitForKeyPress();
            return;
        }
        DisplayFlight(flight);
    }

    public static void DisplayFlight(FlightModel Flight)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(new Color(184, 123, 74)))
            .Expand();

        table.AddColumn(new TableColumn("[rgb(134,64,0)]Flight Information[/]").Centered());

        var profileData = new Panel($"""
            [rgb(134,64,0)]Airline:[/] [rgb(255,122,0)]{Flight.Airline}[/]
            [rgb(134,64,0)]Airplane Model:[/] [rgb(255,122,0)]{Flight.AirplaneID}[/]
            [rgb(134,64,0)]Departure Airport:[/] [rgb(255,122,0)]{Flight.DepartureAirport}[/]
            [rgb(134,64,0)]Arrival Airport:[/] [rgb(255,122,0)]{Flight.ArrivalAirport}[/]
            [rgb(134,64,0)]Departure Time:[/] [rgb(255,122,0)]{Flight.DepartureTime:yyyy-MM-dd}[/]
            [rgb(134,64,0)]Arrival Time:[/] [rgb(255,122,0)]{Flight.ArrivalTime:yyyy-MM-dd}[/]
            [rgb(134,64,0)]Price:[/] [rgb(255,122,0)]{Flight.Price + "$"}[/]
            [rgb(134,64,0)]FlightStatus:[/] [rgb(255,122,0)]{Flight.FlightStatus}[/]
            """)
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(new Color(184, 123, 74)))
            .Padding(1, 1);

        table.AddRow(profileData);
        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine("\n[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }

    public static string SelectAirplaneIDFromList()
    {
        List<AirplaneModel> airplanes = AirplaneLogic.GetAllAirplanes();

        if (!airplanes.Any())
        {
            AnsiConsole.MarkupLine("[red]No airplanes found in the system.[/]");
            return null;
        }

        var airplaneChoices = airplanes
            .Select(airplane => $"{airplane.AirplaneID} - {airplane.AirplaneName}")
            .ToList();

        string selectedAirplane = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[#864000]Select an airplane:[/]")
                .PageSize(10)
                .HighlightStyle(highlightStyle)
                .AddChoices(airplaneChoices));

        // Extract the AirplaneID from the selection (gets the part before the hyphen)
        return selectedAirplane.Split('-')[0].Trim();
    }
}