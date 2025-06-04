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

    public void SetSeatOccupied(int flightId, string seatId, bool isOccupied)
    {
        string sql = $"UPDATE {FlightSeatsTable} SET IsOccupied = @IsOccupied WHERE FlightID = @FlightID AND SeatID = @SeatID";
        _connection.Execute(sql, new { IsOccupied = isOccupied, FlightID = flightId, SeatID = seatId });
    }

    public void CreateFlightSeats(int flightId, string airplaneId)
    {
        string sql = $@"INSERT INTO {FlightSeatsTable} (FlightID, SeatID, IsOccupied) VALUES (@FlightID, @SeatID, 0)";
        _connection.Execute(sql, new { FlightID = flightId, SeatID = airplaneId });
    }
}
