namespace ProjectB.DataAccess
{
    public interface IAirportAccess
    {
        AirportModel GetAirportByCode(string iataCode);
        List<AirportModel> GetAllAirports();
    }
}