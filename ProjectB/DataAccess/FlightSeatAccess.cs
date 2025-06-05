using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;
using System.Collections.Generic;
using System.Linq;

public class FlightSeatAccess : IFlightSeatAccess
{
    private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private const string Table = "FlightSeats";

    public List<(SeatModel seat, bool isOccupied)> GetSeatsForFlight(int flightId)
    {
        string sql = @"
            SELECT 
                S.*,
                FS.IsOccupied
            FROM 
                Seats S
            INNER JOIN 
                FlightSeats FS ON S.SeatID = FS.SeatID
            WHERE 
                FS.FlightID = @FlightID";

        return _connection.Query<SeatModel, long, (SeatModel seat, bool isOccupied)>(
            sql,
            (seat, isOccupiedLong) => {
                bool isOccupied = isOccupiedLong == 1; 
                return (seat, isOccupied);
            },
            new { FlightID = flightId },
            splitOn: "IsOccupied"
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