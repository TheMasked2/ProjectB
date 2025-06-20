using ProjectB.DataAccess;

public static class PastFlightLogic
{
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    public static IAirplaneAccess AirplaneAccessService { get; set; } = new AirplaneAccess();
    public static IPastFlightAccess PastFlightAccessService { get; set; } = new PastFlightAccess();
    public static ISeatAccess SeatAccessService { get; set; } = new SeatAccess();

    public static List<FlightModel> GetFilteredPastFlights(
        string? origin,
        string? destination,
        DateTime departureDate) => PastFlightAccessService.GetFilteredPastFlights(origin, destination, departureDate);  

    public static List<FlightModel> GetAllPastFlights() => PastFlightAccessService.GetAllPastFlights();

}
