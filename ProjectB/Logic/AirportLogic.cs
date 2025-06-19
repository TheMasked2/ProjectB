using ProjectB.DataAccess;
using Spectre.Console;

public static class AirportLogic
{
    public static IAirportAccess AirportAccessService { get; set; } = new AirportAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));

    public static List<AirportModel> GetAllAirports()
    {
        return AirportAccessService.GetAllAirports();
    }

    public static Table CreateAirportsTable(List<AirportModel> airports = null)
    {
        if (airports == null)
        {
            airports = GetAllAirports();
        }
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();
            
        table.AddColumns(
            "[#864000]IATA Code[/]", 
            "[#864000]Airport[/]", 
            "[#864000]City[/]", 
            "[#864000]Country[/]"
        );
        
        foreach (AirportModel airport in airports)
        {
            table.AddRow(
                airport.IataCode,
                airport.Name,
                airport.City,
                airport.Country
            );
        }
        
        return table;
    }
}