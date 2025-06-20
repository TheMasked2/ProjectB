using Microsoft.VisualBasic;
using Spectre.Console;

public class PastFlightUI
{
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    public static void DisplayFilteredPastFlights()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("Past Flight Search")
                .Centered()
                .Color(Color.Orange1));

        bool hasFilters = false;
        AnsiConsole.MarkupLine("\n[#864000]Enter filter criteria:[/]");

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

        string startDateInput = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Start date (yyyy-MM-dd). Press Enter to fill in today's date:[/]")
                .AllowEmpty()
                .DefaultValue(DateTime.Now.ToString("yyyy-MM-dd"))
                .PromptStyle(highlightStyle));

        DateTime startDate;
        if (!DateTime.TryParse(startDateInput, out startDate))
        {
            AnsiConsole.MarkupLine("[red]Invalid start date format. Please use yyyy-MM-dd.[/]");
            FlightUI.WaitForKeyPress();
            return; // or handle the error as needed
        }
        else if(startDate > DateTime.Now)
        {
            AnsiConsole.MarkupLine("[red]Date cannot be in the future. Please enter a date from before today.[/]");
            FlightUI.WaitForKeyPress();
            return; // or handle the error as needed
        }

        var flights = PastFlightLogic.GetFilteredPastFlights(origin, destination, startDate);

        AnsiConsole.Write(FlightLogic.CreateDisplayableFlightsTable(flights));
        FlightUI.WaitForKeyPress();
    }
}