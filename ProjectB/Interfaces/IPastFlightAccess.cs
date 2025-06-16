namespace ProjectB.DataAccess
{
    public interface IPastFlightAccess
    {
        void WritePastFlight(FlightModel flight);
        List<FlightModel> GetAllPastFlights();
        List<FlightModel> GetFilteredPastFlights(
            string? origin,
            string? destination,
            DateTime departureDate);
        List<int> GetOldPastFlightIDs(DateTime monthAgo);
        void DeleteOldPastFlights(List<int> flightIDs);
    }
}
