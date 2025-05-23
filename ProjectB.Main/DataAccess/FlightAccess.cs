using Microsoft.Data.Sqlite;
using Dapper;

public static class FlightAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private static string Table = "FLIGHTS";

    /// <summary>
    /// Inserts a new flight into the database.
    /// </summary>
    /// <param name="flight">The flight to insert.</param>
    public static void Write(FlightModel flight)
    {
        string sql = $@"INSERT INTO {Table} 
                        (FlightID, Airline, AirplaneID, AvailableSeats, DepartureAirport, ArrivalAirport, 
                         DepartureTime, ArrivalTime, FlightStatus) 
                        VALUES 
                        (@flightID, @airline, @airplaneID, @availableSeats, 
                         @departureAirport, @arrivalAirport, @departureTime, @arrivalTime, @flightStatus)";
        _connection.Execute(sql,
            new
            {
                @flightID = flight.FlightID,
                @airline = flight.Airline,
                @airplaneID = flight.AirplaneID,
                @availableSeats = flight.AvailableSeats,
                @departureAirport = flight.DepartureAirport,
                @arrivalAirport = flight.ArrivalAirport,
                @departureTime = flight.DepartureTime,
                @arrivalTime = flight.ArrivalTime,
                @flightStatus = flight.FlightStatus
            }
        );
    }

    /// <summary>
    /// Retrieves a flight by its ID.
    /// </summary>
    /// <param name="flightId">The ID of the flight to retrieve.</param>
    /// <returns>The flight with the specified ID, or null if not found.</returns>
    public static FlightModel GetById(int flightId)
    {
        string sql = $@"SELECT 
                            FlightID,
                            Airline,
                            AirplaneID,
                            AvailableSeats,
                            DepartureAirport,
                            ArrivalAirport,
                            DepartureTime,
                            ArrivalTime,
                            Price,
                            FlightStatus
                        FROM {Table} WHERE FlightID = @FlightId";
    
        return _connection.QueryFirstOrDefault<FlightModel>(sql, new { FlightId = flightId });
    }

    /// <summary>
    /// Updates an existing flight in the database.
    /// </summary>
    /// <param name="flight">The flight to update.</param>
    public static void Update(FlightModel flight)
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
                        WHERE FlightID = @FlightID";

        int rowsAffected = _connection.Execute(sql,
            new
            {
                @airline = flight.Airline,
                @airplaneID = flight.AirplaneID,
                @availableSeats = flight.AvailableSeats,
                @departureAirport = flight.DepartureAirport,
                @arrivalAirport = flight.ArrivalAirport,
                @departureTime = flight.DepartureTime,
                @arrivalTime = flight.ArrivalTime,
                @flightStatus = flight.FlightStatus
            }
        );
    
        if (rowsAffected == 0)
        {
            Console.WriteLine("No rows were updated. Check if the Flight ID exists in the database.");
        }
    }

    /// <summary>
    /// Deletes a flight from the database.
    /// </summary>
    /// <param name="flightId">The ID of the flight to delete.</param>
    public static void Delete(int flightId)
    {
        string sql = $"DELETE FROM {Table} WHERE FlightID = @FlightId";
        _connection.Execute(sql, new { FlightId = flightId });
    }

    /// <summary>
    /// Retrieves all flights from the database.
    /// </summary>
    /// <returns>A list of all flights.</returns>
    public static List<FlightModel> GetAllFlightData()
    {
        try
        {
            string sql = $@"SELECT 
                            FlightID,
                            Airline,
                            AirplaneID,
                            AvailableSeats,
                            DepartureAirport,
                            ArrivalAirport,
                            DepartureTime,
                            ArrivalTime,
                            Price,
                            FlightStatus
                          FROM {Table}";
            
            return _connection.Query<FlightModel>(sql).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing flights: {ex.Message}");
            return new List<FlightModel>();
        }
    }
}