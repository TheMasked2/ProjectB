using Spectre.Console;

public class FlightModel
{
    public int FlightID { get; set; } // Primary Key
    public string Airline { get; set; }
    public string AirplaneID { get; set; } // Foreign Key
    public string DepartureAirport { get; set; } // Foreign Key
    public string ArrivalAirport { get; set; } // Foreign Key
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string FlightStatus { get; set; }
    
    public FlightModel() { }
}
