using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class FlightSeatAccess : IFlightSeatAccess
{
    private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private const string FlightSeatsTable = "FlightSeats";
    private const string SeatsTable = "Seats";

    public List<SeatModel> GetSeatsForFlight(int flightId)
    {
        // This SQL query retrieves all seat information for a specific flight.
        // It combines data from two tables: FlightSeats (fs) and Seats (s).
        // FlightSeats contains flight-specific seat data,
        // while Seats contains the general seat information.
        // The INNER JOIN connects these tables using SeatID to get complete seat details.
        // Results are ordered by row number and seat position for logical display.
        string sql = $@"
            SELECT 
                s.SeatID,
                s.AirplaneID,
                s.RowNumber,
                s.SeatPosition,
                s.SeatType,
                s.Price,
                fs.IsOccupied
            FROM {FlightSeatsTable} fs
            INNER JOIN {SeatsTable} s ON fs.SeatID = s.SeatID
            WHERE fs.FlightID = @FlightID
            ORDER BY s.RowNumber, s.SeatPosition
        ";
        return _connection.Query<SeatModel>(sql, new { FlightID = flightId }).ToList();
    }

    public bool HasAnySeatsForFlight(int flightId)
    {
        string sql = $"SELECT 1 FROM {FlightSeatsTable} WHERE FlightID = @FlightID LIMIT 1";
        return _connection.QueryFirstOrDefault<int?>(sql, new { FlightID = flightId }) != null;
    }

    public void BulkCreateAllFlightSeats(List<(int flightId, string airplaneId)> toBackfill)
    {
        foreach (var (flightId, airplaneId) in toBackfill)
        {
            string sql = $@"INSERT INTO {FlightSeatsTable} (FlightID, SeatID, IsOccupied) VALUES (@FlightID, @SeatID, 0)";
            _connection.Execute(sql, new { FlightID = flightId, SeatID = airplaneId });
        }
    }

    public void SetSeatOccupancy(int flightId, string seatId, bool isOccupied)
    {
        string sql = $"UPDATE {FlightSeatsTable} SET IsOccupied = @IsOccupied WHERE FlightID = @FlightID AND SeatID = @SeatID";
        _connection.Execute(sql, new { IsOccupied = isOccupied, FlightID = flightId, SeatID = seatId });
    }

    public void CreateFlightSeats(int flightId, string airplaneId)
    {
        string sql = $@"INSERT INTO {FlightSeatsTable} (FlightID, SeatID, IsOccupied) VALUES (@FlightID, @SeatID, 0)";
        _connection.Execute(sql, new { FlightID = flightId, SeatID = airplaneId });
    }

    public void DeletePastFlightSeatsByFlightIDs(List<int> flightIDs)
    {
        string sql = $@"DELETE FROM {FlightSeatsTable} WHERE FlightID IN @FlightIDs";
        var parameters = new { FlightIDs = flightIDs };
        _connection.Execute(sql, parameters);
    }

    public void DeleteFlightSeatsByFlightID(int flightId)
    {
        string sql = $@"DELETE FROM {FlightSeatsTable} WHERE FlightID = @FlightID";
        var parameters = new { FlightID = flightId };
        _connection.Execute(sql, parameters);
    }

    public int GetAvailableSeatCountByClass(int flightID, string airplaneID, string seatClass)
    {
        // This SQL query counts avialable seats for a specific flight and seat class.
        // It uses the FlightSeatsTable (fs) to find seats for the given flight,
        // and the SeatsTable (s) to filter by seat type (class).
        // It then filters by the given FlightID, SeatType (class),
        // and returns the count of unoccupied seats (IsOccupied = 0).
        string sql = $@"
            SELECT COUNT(fs.SeatID)
            FROM {FlightSeatsTable} fs
            INNER JOIN {SeatsTable} s ON fs.SeatID = s.SeatID AND s.AirplaneID = @AirplaneID 
            WHERE fs.FlightID = @FlightID
              AND s.SeatType = @SeatClass
              AND fs.IsOccupied = 0";
        var parameters = new { FlightID = flightID, AirplaneID = airplaneID, SeatClass = seatClass };

        return _connection.ExecuteScalar<int>(sql, parameters);
    }
}