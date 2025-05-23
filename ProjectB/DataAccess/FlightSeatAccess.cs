using Dapper;
using Microsoft.Data.Sqlite;

public static class FlightSeatAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private static string Table = "FlightSeats";

    // Call this when creating a new flight
    public static void CreateFlightSeats(int flightId, string airplaneId)
    {
        var seats = SeatAccess.GetByAircraft(airplaneId);
        foreach (var seat in seats)
        {
            string sql = $@"INSERT INTO {Table} (FlightID, SeatID, IsOccupied)
                            VALUES (@FlightID, @SeatID, 0)";
            _connection.Execute(sql, new { FlightID = flightId, SeatID = seat.SeatID });
        }
    }

    // Bulk insert seats for a flight
    public static void BulkCreateFlightSeats(int flightId, string airplaneId)
    {
        var seats = SeatAccess.GetByAircraft(airplaneId);
        var flightSeats = seats.Select(seat => new
        {
            FlightID = flightId,
            SeatID = seat.SeatID,
            IsOccupied = 0
        }).ToList();

        string sql = $@"INSERT INTO {Table} (FlightID, SeatID, IsOccupied)
                        VALUES (@FlightID, @SeatID, @IsOccupied)";
        _connection.Execute(sql, flightSeats);
    }

    // Bulk insert seats for multiple flights in a single transaction
    public static void BulkCreateAllFlightSeats(IEnumerable<(int FlightID, string AirplaneID)> flights)
    {
        using (var transaction = _connection.BeginTransaction())
        {
            string sql = $@"INSERT INTO {Table} (FlightID, SeatID, IsOccupied)
                            VALUES (@FlightID, @SeatID, 0)";
            foreach (var (flightId, airplaneId) in flights)
            {
                var seats = SeatAccess.GetByAircraft(airplaneId);
                var flightSeats = seats.Select(seat => new
                {
                    FlightID = flightId,
                    SeatID = seat.SeatID,
                    IsOccupied = 0
                }).ToList();

                _connection.Execute(sql, flightSeats, transaction: transaction);
            }
            transaction.Commit();
        }
    }

    // Get seat info for a flight (join with Seats)
    public static List<(SeatModel seat, bool isOccupied)> GetSeatsForFlight(int flightId)
    {
        string sql = @"
            SELECT 
                s.SeatID, s.AirplaneID, s.RowNumber, s.SeatPosition, s.SeatType, s.Price,
                fs.IsOccupied AS FlightIsOccupied
            FROM FlightSeats fs
            JOIN Seats s ON fs.SeatID = s.SeatID
            WHERE fs.FlightID = @FlightID
            ORDER BY s.RowNumber, s.SeatPosition";
        // Accept FlightIsOccupied as long and convert to bool
        return _connection.Query<SeatModel, long, (SeatModel, bool)>(
            sql,
            (seat, flightIsOccupied) => (seat, flightIsOccupied != 0),
            new { FlightID = flightId },
            splitOn: "FlightIsOccupied"
        ).ToList();
    }

    // Fast check: does this flight have any seats in FlightSeats?
    public static bool HasAnySeatsForFlight(int flightId)
    {
        string sql = $"SELECT 1 FROM {Table} WHERE FlightID = @FlightID LIMIT 1";
        return _connection.QueryFirstOrDefault<int?>(sql, new { FlightID = flightId }) != null;
    }

    // Update occupancy
    public static void SetSeatOccupied(int flightId, string seatId, bool occupied)
    {
        string sql = $"UPDATE {Table} SET IsOccupied = @IsOccupied WHERE FlightID = @FlightID AND SeatID = @SeatID";
        _connection.Execute(sql, new { IsOccupied = occupied, FlightID = flightId, SeatID = seatId });
    }
}
