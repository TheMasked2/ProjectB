namespace ProjectB.DataAccess
{
    public interface IPastFlightAccess
    {
        void DeletePastFlights(DateTime monthAgo);
        void WritePastFlight(FlightModel flight);
        List<FlightModel> GetAllPastFlights();
        List<FlightModel> GetFilteredPastFlights(
            string? origin,
            string? destination,
            DateTime departureDate);
    }
}
