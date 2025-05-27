public interface IAirplaneAccess
{
    AirplaneModel GetAirplaneById(string airplaneId);
    List<AirplaneModel> GetAllAirplanes();
    void AddAirplane(AirplaneModel airplane);
    void UpdateAirplane(AirplaneModel airplane);
    void DeleteAirplane(string airplaneId);
}