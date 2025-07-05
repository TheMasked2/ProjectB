namespace ProjectB.DataAccess
{
    public interface IFlightAccess : IGenericAccess<FlightModel, int>
    {
        List<FlightModel> GetPastFlights(DateTime currentDate);
        List<FlightModel> GetUpcomingFlights(DateTime departingSoonDate);
        List<FlightModel> GetFilteredFlights(
            string? origin,
            string? destination,
            DateTime departureDate);
    }
}