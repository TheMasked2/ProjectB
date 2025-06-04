using Spectre.Console;

public class FlightModel
{
    public int FlightID { get; set; } // Primary Key
    public string Airline { get; set; }
    public string AirplaneID { get; set; } // Foreign Key
    public int AvailableSeats { get; set; }
    public string DepartureAirport { get; set; } // Foreign Key
    public string ArrivalAirport { get; set; } // Foreign Key
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public float Price { get; set; }
    public string FlightStatus { get; set; }

    public FlightModel(
        int flightId,
        string airline,
        string airplaneId,
        int availableSeats,
        string departureAirport,
        string arrivalAirport,
        DateTime departureTime,
        DateTime arrivalTime,
        float price,
        string status
    )
    {
        FlightID = flightId;
        Airline = airline;
        AirplaneID = airplaneId;
        AvailableSeats = availableSeats;
        DepartureAirport = departureAirport;
        ArrivalAirport = arrivalAirport;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        Price = price;
        FlightStatus = status;
    }

    public FlightModel() { }

    public override string ToString()
    {
        // return
        //     $"---------------------------------------------------\n" +
        //     $"Flight ID: {FlightId}\n" +
        //     $"Airline: {Airline}\n" +
        //     $"Departure Time: {DepartureTime:yyyy-MM-dd HH:mm}\n" +
        //     $"Arrival Time: {ArrivalTime:yyyy-MM-dd HH:mm}\n" +
        //     $"Price: {Price}\n" +
        //     $"Available Seats: {AvailableSeats}\n" +
        //     $"Status: {Status}\n" +
        //     $"---------------------------------------------------";

        return $"""
           [#864000]Flight Details:[/]
           [#FF7A00]ID:[/] {FlightID}
           [#FF7A00]Airline:[/] {Airline}
           [#FF7A00]Route:[/] {DepartureAirport} → {ArrivalAirport}
           [#FF7A00]Departure:[/] {DepartureTime:g}
           [#FF7A00]Arrival:[/] {ArrivalTime:g}
           [#FF7A00]Available Seats:[/] {AvailableSeats}
           [#FF7A00]Status:[/] {FlightStatus}
           [#FF7A00]Price:[/] €{Price}
           """;
    }
}
