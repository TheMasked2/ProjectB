using Microsoft.Data.Sqlite;
using Dapper;

public static class SeatAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private static string Table = "Seats";

    public static void BulkInsertSeats(List<SeatModel> seats)
    {
        string sql = $@"INSERT INTO {Table} 
                        (AirplaneID, RowNumber, SeatPosition, SeatType, Price, IsOccupied) 
                        VALUES (@AirplaneID, @RowNumber, @SeatPosition, @SeatType, @Price, @IsOccupied)";

        _connection.Execute(sql, seats);
    }

    public static void Write(SeatModel seat)
    {
        string sql = $@"INSERT INTO {Table}
                        (AirplaneID, RowNumber, SeatPosition, SeatType, Price, IsOccupied)
                        VALUES (@AirplaneID, @RowNumber, @SeatPosition, @SeatType, @Price, @IsOccupied)";
        _connection.Execute(sql, seat);
    }

    public static SeatModel GetSeatById(string seatId)
    {
        string sql = $@"SELECT 
                            SeatID, 
                            AirplaneID, 
                            RowNumber, 
                            SeatPosition, 
                            SeatType,
                            Price,
                            IsOccupied 
                        FROM {Table} 
                        WHERE SeatID = @SeatId";

        return _connection.QueryFirstOrDefault<SeatModel>(sql, new { SeatId = seatId });
    }

    public static List<SeatModel> GetByAircraft(string airplaneId)
    {
        string sql = $@"SELECT 
                            SeatID, 
                            AirplaneID, 
                            RowNumber, 
                            SeatPosition, 
                            SeatType,
                            Price,
                            IsOccupied 
                        FROM {Table} 
                        WHERE AirplaneID = @AirplaneID
                        ORDER BY RowNumber, SeatPosition";

        return _connection.Query<SeatModel>(sql, new { AirplaneID = airplaneId }).ToList();
    }

    public static void Update(SeatModel seat)
    {
        string sql = $@"UPDATE {Table}
                        SET AirplaneID = @AirplaneID, 
                            RowNumber = @RowNumber, 
                            SeatPosition = @SeatPosition, 
                            SeatType = @SeatType,
                            Price = @Price,
                            IsOccupied = @IsOccupied
                        WHERE SeatID = @SeatID";

        int rowsAffected = _connection.Execute(sql, seat);

        if (rowsAffected == 0)
        {
            Console.WriteLine("No rows were updated. Check if the Seat ID exists in the database.");
        }
    }

    public static void Delete(string seatId)
    {
        string sql = $"DELETE FROM {Table} WHERE SeatID = @SeatId";
        _connection.Execute(sql, new { SeatId = seatId });
    }

    public static List<SeatModel> GetAllSeats()
    {
        try
        {
            string sql = $@"SELECT 
                                SeatID, 
                                AirplaneID, 
                                RowNumber, 
                                SeatPosition, 
                                SeatType,
                                Price,
                                IsOccupied 
                           FROM {Table}";
            return _connection.Query<SeatModel>(sql).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving seats: {ex.Message}");
            return new List<SeatModel>();
        }
    }
}