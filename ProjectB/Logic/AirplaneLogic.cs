using Spectre.Console;
using ProjectB.DataAccess;
public static class AirplaneLogic
{
    public static IAirplaneAccess AirplaneAccessService { get; set; } = new AirplaneAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static AirplaneModel GetAirplaneByID(string airplaneID)
    {
        AirplaneModel airplane = AirplaneAccessService.GetAirplaneByID(airplaneID);
        return airplane;
    }
}
