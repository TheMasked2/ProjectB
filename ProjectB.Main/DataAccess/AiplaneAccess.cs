using Microsoft.Data.Sqlite;
using Dapper;

public static class AiplaneAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private static string Table = "AIRPLANE";

    /// <summary>
    /// Inserts a new airplane into the database.
    /// </summary>
    /// <param name="airplane">The airplane to insert.</param>
    public static AirplaneModel GetAirplaneData(string airplaneName)
    {
        string sql = $@"SELECT 
                            AirplaneID AS AirplaneId, 
                            AirplaneName AS AirplaneName, 
                            TotalSeats AS Capacity 
                        FROM {Table} WHERE AirplaneName = @AirplaneName";
        var result = _connection.QueryFirstOrDefault<AirplaneModel>(sql, new { @AirplaneName = airplaneName });
        return result;

    }
}