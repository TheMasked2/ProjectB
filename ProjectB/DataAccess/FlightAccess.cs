using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class FlightAccess : GenericAccess<FlightModel, int>, IFlightAccess
{
    protected override string Table => "FLIGHTS";
    protected override string PrimaryKey => "FlightID";
    public override void Insert(FlightModel flight)
    {
        string sql = $@"INSERT INTO {Table}
                        (Airline, 
                        AirplaneID, 
                        AvailableSeats, 
                        DepartureAirport, 
                        ArrivalAirport, 
                        DepartureTime, 
                        ArrivalTime, 
                        FlightStatus) 
                        VALUES 
                        (@Airline, 
                        @AirplaneID, 
                        @AvailableSeats, 
                        @DepartureAirport, 
                        @ArrivalAirport, 
                        @DepartureTime, 
                        @ArrivalTime,
                        @FlightStatus)";
        _connection.Execute(sql, flight);
    }

    public override void Update(FlightModel flight)
    {
        string sql = $@"UPDATE {Table} 
                        SET Airline = @Airline, 
                            AirplaneID = @AirplaneID, 
                            AvailableSeats = @AvailableSeats, 
                            DepartureAirport = @DepartureAirport, 
                            ArrivalAirport = @ArrivalAirport, 
                            DepartureTime = @DepartureTime, 
                            ArrivalTime = @ArrivalTime,
                            FlightStatus = @FlightStatus 
                        WHERE FlightID = @FlightID";
        _connection.Execute(sql, flight);
    }

    public List<FlightModel> GetPastFlights(DateTime currentDate)
    {
        string sql = $@"SELECT * FROM {Table} 
                        WHERE DepartureTime < @CurrentTime
                        AND FlightStatus = 'Departed'";

        return _connection.Query<FlightModel>(sql, new { CurrentTime = currentDate }).ToList();
    }

    public List<FlightModel> GetUpcomingFlights(DateTime departingSoonDate)
    {
        string sql = $@"SELECT * FROM {Table} 
                        WHERE DepartureTime <= @SoonDate
                        AND FlightStatus != 'Departed'";
                        

        return _connection.Query<FlightModel>(sql, new { SoonDate = departingSoonDate }).ToList();
    }

    public List<FlightModel> GetFilteredFlights(
        string? origin,
        string? destination,
        DateTime departureDate)
    {
        string sql = $@"SELECT * FROM {Table}
                        WHERE date(DepartureTime) = date(@DepartureDate)
                        AND DepartureAirport LIKE @Origin
                        AND ArrivalAirport LIKE @Destination
                        AND FlightStatus != 'Departed'";
        
        var parameters = new
        {
            DepartureDate = departureDate,
            Origin = string.IsNullOrEmpty(origin) ? "%" : origin,
            Destination = string.IsNullOrEmpty(destination) ? "%" : destination
        };

        return _connection.Query<FlightModel>(sql, parameters).ToList();
    }

    // You will also need to add the other specific methods from the interface here
    // e.g., DeleteFlightsByIDs and GetOldDepartedFlightIDs
}