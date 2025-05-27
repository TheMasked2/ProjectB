using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class AirplaneAccess : IAirplaneAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private const string Table = "AIRPLANE";

    /// <summary>
    /// Inserts a new airplane into the database.
    /// </summary>
    /// <param name="airplane">The airplane to insert.</param>
    public AirplaneModel GetAirplaneData(string airplaneID)
    {
        string sql = $@"SELECT 
                            AirplaneID,
                            AirplaneName,
                            TotalSeats
                        FROM {Table} WHERE AirplaneID = @AirplaneID";
        var result = _connection.QueryFirstOrDefault<AirplaneModel>(sql, new { @AirplaneID = airplaneID });
        return result;

    }
    public AirplaneModel GetAirplaneById(string airplaneId)
    {
        string sql = $@"SELECT * FROM {Table} WHERE AirplaneID = @AirplaneId";
        var result = _connection.QueryFirstOrDefault<AirplaneModel>(sql, new { AirplaneId = airplaneId });
        return result;
    }
}