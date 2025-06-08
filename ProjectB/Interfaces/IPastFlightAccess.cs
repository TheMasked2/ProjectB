namespace ProjectB.DataAccess
{
    public interface IPastFlightAccess
    {
        void DeletePastFlights(DateTime monthAgo);
        void WritePastFlight(FlightModel flight);
        List<FlightModel> GetPastFlights(DateTime currentDate);
    }
}
