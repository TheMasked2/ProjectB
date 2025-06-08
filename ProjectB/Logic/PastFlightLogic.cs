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
        DateTime departureDate)
    {
        var flights = FlightLogic.GetPastFlights(departureDate);

        if (!string.IsNullOrEmpty(origin))
            flights = flights.Where(f => f.DepartureAirport.Equals(origin, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrEmpty(destination))
            flights = flights.Where(f => f.ArrivalAirport.Equals(destination, StringComparison.OrdinalIgnoreCase)).ToList();

        flights = flights.Where(f => f.DepartureTime.Date == departureDate.Date).ToList();

        return flights;
    }
}
