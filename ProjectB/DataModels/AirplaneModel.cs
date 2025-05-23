public class AirplaneModel
{
    public string AirplaneID { get; set; } // Primary Key
    public string AirplaneName { get; set; }
    public int TotalSeats { get; set; }

    public AirplaneModel(string airplaneId, string airplaneName, int totalSeats)
    {
        AirplaneID = airplaneId;
        AirplaneName = airplaneName;
        TotalSeats = totalSeats;
    }

    public AirplaneModel() { }

    public override string ToString()
    {
        return $"Airplane ID: {AirplaneID}\n" +
               $"Airplane Name: {AirplaneID}\n" +
               $"TotalSeats: {TotalSeats}";
    }
}