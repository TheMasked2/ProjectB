using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;


public class FlightSeatAccess : IFlightSeatAccess
{
    private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private const string Table = "FlightSeats";

    public List<(SeatModel seat, bool isOccupied)> GetSeatsForFlight(int flightId)
    {
        string sql = @"SELECT *, IsOccupied as isOccupied FROM FlightSeats WHERE FlightID = @FlightID";
        var result = _connection.Query<SeatModel>(sql, new { FlightID = flightId })
            .Select(s => (s, s.IsOccupied)).ToList();
        return result;
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

    public void SetSeatOccupied(int flightId, string seatId, bool isOccupied)
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
