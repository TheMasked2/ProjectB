namespace ProjectB.DataAccess
{
    public interface IAirplaneAccess
    {
        AirplaneModel GetAirplaneById(string airplaneId);
        AirplaneModel GetAirplaneData(string airplaneID);
    }
}