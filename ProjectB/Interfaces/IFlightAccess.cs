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
        void DeleteFlightsByIDs(List<int> flightIDs);
        List<int> GetOldDepartedFlightIDs(DateTime monthAgo);
    }
}