using Spectre.Console;
using ProjectB.DataAccess;
using Microsoft.VisualBasic;
public static class PastFlightLogic
{
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    public static IAirplaneAccess AirplaneAccessService { get; set; } = new AirplaneAccess();
    public static IPastFlightAccess PastFlightAccessService { get; set; } = new PastFlightAccess();
    public static ISeatAccess SeatAccessService { get; set; } = new SeatAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static List<FlightModel> GetFilteredPastFlights(
        string? origin,
        string? destination,
        DateTime departureDate) => PastFlightAccessService.GetFilteredPastFlights(origin, destination, departureDate);  

    public static List<FlightModel> GetAllPastFlights() => PastFlightAccessService.GetAllPastFlights();

}
