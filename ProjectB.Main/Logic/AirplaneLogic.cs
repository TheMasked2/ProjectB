using Spectre.Console;

public static class AirplaneLogic
{
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    /// <summary>
    /// Retrieves airplane information by name.
    /// </summary>
    /// <returns>A list of all airplanes.</returns>
    public static AirplaneModel GetAllAirplanes(string airplaneID)
    {
        try
        {
            AirplaneModel result = AirplaneAccess.GetAirplaneData(airplaneID);
            if (result == null)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Airplane with ID {airplaneID} not found.");
                return null;
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]Airplane found:\n[/] {result}");
                return result;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return null;
        }

    }
}