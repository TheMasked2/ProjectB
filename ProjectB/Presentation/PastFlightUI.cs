using Microsoft.VisualBasic;
using Spectre.Console;

public class PastFlightUI
{
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));
    public static void DisplayFilteredPastFlights()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("Past Flight Search")
                .Centered()
                .Color(Color.Orange1));

        bool hasFilters = false;
        AnsiConsole.MarkupLine("\n[#864000]Enter filter criteria. The date will default to today.:[/]");

        string origin = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Origin airport (e.g., LAX). Press Enter to skip:[/]")
                .AllowEmpty()
                .PromptStyle(highlightStyle));

        hasFilters |= !string.IsNullOrWhiteSpace(origin);

        string destination = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Destination airport (e.g., JFK). Press Enter to skip:[/]")
                .AllowEmpty()
                .PromptStyle(highlightStyle));
        hasFilters |= !string.IsNullOrWhiteSpace(destination);

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
        hasFilters = true;

        AnsiConsole.MarkupLine("[red]\nSeat class will be standard to view past flights.\nPress Enter to continue.[/]");
        Console.ReadLine(); // Wait for user to acknowledge the seat class

        string seatClass = "Standard"; // Default value

        var flights = PastFlightLogic.GetFilteredPastFlights(origin, destination, startDate);

        if (!hasFilters)
        {
            AnsiConsole.MarkupLine("\n[yellow]No filters applied - showing all flights[/]");
        }

        AnsiConsole.Write(FlightLogic.DisplayFilteredFlights(flights, seatClass));
        FlightUI.WaitForKeyPress();
    }
}