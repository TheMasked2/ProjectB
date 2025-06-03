namespace ProjectB.DataAccess
{
    public interface IFlightAccess
    {
        List<FlightModel> GetAllFlightData();
        FlightModel GetById(int flightId);
        void Write(FlightModel flight);
        void Update(FlightModel flight);
        void Delete(int flightId);
    }
}