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

    public AirplaneModel GetAirplaneByID(string airplaneID)
    {
        string sql = $@"SELECT * FROM {Table} WHERE AirplaneID = @AirplaneId";
        AirplaneModel? result = _connection.QueryFirstOrDefault<AirplaneModel>(sql, new { AirplaneId = airplaneID });
        return result;
    }

    public List<AirplaneModel> GetAirplanes()
    {
        string sql = $@"SELECT AirplaneID, AirplaneName FROM {Table}";
        IEnumerable<AirplaneModel> result = _connection.Query<AirplaneModel>(sql);
        return result.ToList();
    }
}
