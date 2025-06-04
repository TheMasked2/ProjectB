using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class SeatAccess : ISeatAccess
{

    private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private const string Table = "SEATS";
    public float GetSeatClassPrice(string airplaneID, string seatClass)
    {

        string sql = $@"SELECT Price FROM {Table} WHERE AirplaneID = @AirplaneID AND SeatType = @SeatClass";
        return _connection.QueryFirstOrDefault<float>(sql, new { AirplaneID = airplaneID, SeatClass = seatClass });
    }
}