using ProjectB.DataAccess;
public static class AirplaneLogic
{
    public static IAirplaneAccess AirplaneAccessService { get; set; } = new AirplaneAccess();
    public static AirplaneModel? GetAirplaneByID(string airplaneID)
    {
        AirplaneModel? airplane = AirplaneAccessService.GetById(airplaneID);
        return airplane;
    }

    public static List<AirplaneModel>? GetAllAirplanes()
    {
        List<AirplaneModel>? airplanes = AirplaneAccessService.GetAll();
        return airplanes;
    }
}
