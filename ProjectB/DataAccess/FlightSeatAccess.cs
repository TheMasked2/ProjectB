using Microsoft.Data.Sqlite;
using Dapper;

public class FlightSeatAccess
{
    private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private const string Table = "FlightSeats";

    public List<(SeatModel seat, bool isOccupied)> GetSeatsForFlight(int flightId)
    {
        string sql = @"SELECT * FROM FlightSeats WHERE FlightID = @FlightID";
        return _connection.Query<SeatModel, bool, (SeatModel, bool)>(
            sql,
            (seat, isOccupied) => (seat, isOccupied),
            new { FlightID = flightId }
        ).ToList();
    }

    public bool HasAnySeatsForFlight(int flightId)
    {
        string sql = $"SELECT 1 FROM {Table} WHERE FlightID = @FlightID LIMIT 1";
        return _connection.QueryFirstOrDefault<int?>(sql, new { FlightID = flightId }) != null;
    }

    public void BulkCreateAllFlightSeats(List<(int flightId, string airplaneId)> toBackfill)
    {
        foreach (var (flightId, airplaneId) in toBackfill)
        {
            string sql = $@"INSERT INTO {Table} (FlightID, SeatID, IsOccupied) VALUES (@FlightID, @SeatID, 0)";
            _connection.Execute(sql, new { FlightID = flightId, SeatID = airplaneId });
        }
    }

    public void SetSeatOccupied(int flightId, int seatId, bool isOccupied)
    {
        string sql = $"UPDATE {Table} SET IsOccupied = @IsOccupied WHERE FlightID = @FlightID AND SeatID = @SeatID";
        _connection.Execute(sql, new { IsOccupied = isOccupied, FlightID = flightId, SeatID = seatId });
    }

    public void CreateFlightSeats(int flightId, string airplaneId)
    {
        string sql = $@"INSERT INTO {Table} (FlightID, SeatID, IsOccupied) VALUES (@FlightID, @SeatID, 0)";
        _connection.Execute(sql, new { FlightID = flightId, SeatID = airplaneId });
    }
}
