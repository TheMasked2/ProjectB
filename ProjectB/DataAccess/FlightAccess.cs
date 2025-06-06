using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class FlightAccess : IFlightAccess
{
    private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private const string Table = "FLIGHTS";

    /// <summary>
    /// Inserts a new flight into the database.
    /// </summary>
    /// <param name="flight">The flight to insert.</param>
    public void Write(FlightModel flight)
    {
        string sql = $@"INSERT INTO {Table} 
                        (FlightID, Airline, AirplaneID, AvailableSeats, DepartureAirport, ArrivalAirport, 
                         DepartureTime, ArrivalTime, FlightStatus) 
                        VALUES 
                        (@flightID, @airline, @airplaneID, @availableSeats, 
                         @departureAirport, @arrivalAirport, @departureTime, @arrivalTime, @flightStatus)";
        _connection.Execute(sql, new {
            flightID = flight.FlightID,
            airline = flight.Airline,
            airplaneID = flight.AirplaneID,
            availableSeats = flight.AvailableSeats,
            departureAirport = flight.DepartureAirport,
            arrivalAirport = flight.ArrivalAirport,
            departureTime = flight.DepartureTime,
            arrivalTime = flight.ArrivalTime,
            flightStatus = flight.FlightStatus
        });
    }

    /// <summary>
    /// Retrieves a flight by its ID.
    /// </summary>
    /// <param name="flightId">The ID of the flight to retrieve.</param>
    /// <returns>The flight with the specified ID, or null if not found.</returns>
    public FlightModel GetById(int flightId)
    {
        string sql = $@"SELECT * FROM {Table} WHERE FlightID = @FlightId";
        return _connection.QueryFirstOrDefault<FlightModel>(sql, new { FlightId = flightId });
    }

    /// <summary>
    /// Updates an existing flight in the database.
    /// </summary>
    /// <param name="flight">The flight to update.</param>
    public void Update(FlightModel flight)
    {
        string sql = $@"UPDATE {Table} 
                        SET Airline = @airline, 
                            AirplaneID = @airplaneID, 
                            AvailableSeats = @availableSeats, 
                            DepartureAirport = @departureAirport, 
                            ArrivalAirport = @arrivalAirport, 
                            DepartureTime = @departureTime, 
                            ArrivalTime = @arrivalTime,
                            FlightStatus = @flightStatus 
                        WHERE FlightID = @flightID";
        _connection.Execute(sql, new {
            flightID = flight.FlightID,
            airline = flight.Airline,
            airplaneID = flight.AirplaneID,
            availableSeats = flight.AvailableSeats,
            departureAirport = flight.DepartureAirport,
            arrivalAirport = flight.ArrivalAirport,
            departureTime = flight.DepartureTime,
            arrivalTime = flight.ArrivalTime,
            flightStatus = flight.FlightStatus
        });
    }

    /// <summary>
    /// Deletes a flight from the database.
    /// </summary>
    /// <param name="flightId">The ID of the flight to delete.</param>
    public void Delete(int flightId)
    {
        string sql = $"DELETE FROM {Table} WHERE FlightID = @FlightId";
        _connection.Execute(sql, new { FlightId = flightId });
    }

    /// <summary>
    /// Retrieves all flights from the database.
    /// </summary>
    /// <returns>A list of all flights.</returns>
    public List<FlightModel> GetAllFlightData()
    {
        string sql = $@"SELECT * FROM {Table}";
        var result = _connection.Query<FlightModel>(sql).ToList();
        return result;
    }

    public List<FlightModel> GetPastFlights(DateTime currentDate)
    {
        string sql = $@"SELECT * FROM {Table} 
                        WHERE DepartureTime < @CurrentTime";

        return _connection.Query<FlightModel>(sql, new { CurrentTime = currentDate }).ToList();
    }

    public List<FlightModel> GetUpcomingFlights(DateTime departingSoonDate)
    {
        string sql = $@"SELECT * FROM {Table} 
                        WHERE DepartureTime <= @SoonDate";

        return _connection.Query<FlightModel>(sql, new { SoonDate = departingSoonDate }).ToList();
    }
}