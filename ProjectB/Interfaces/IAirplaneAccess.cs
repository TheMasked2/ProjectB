namespace ProjectB.DataAccess
{
    public interface IAirplaneAccess
    {
        AirplaneModel GetAirplaneByID(string airplaneID);
    }
}