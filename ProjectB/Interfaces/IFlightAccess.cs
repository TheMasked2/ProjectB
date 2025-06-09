namespace ProjectB.DataAccess
{
    public interface IFlightAccess
    {
        List<FlightModel> GetAllFlightData();
        FlightModel GetById(int flightId);
        void Write(FlightModel flight);
        void Update(FlightModel flight);
        void Delete(int flightId);
        List<FlightModel> GetPastFlights(DateTime currentDate);
        List<FlightModel> GetUpcomingFlights(DateTime departingSoonDate);
        List<FlightModel> GetFilteredFlights(
            string? origin,
            string? destination,
            DateTime departureDate);
    }
}