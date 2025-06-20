using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class AirplaneAccess : IAirplaneAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private const string Table = "AIRPLANE";

    public AirplaneModel GetAirplaneByID(string airplaneID)
    {
        string sql = $@"SELECT * FROM {Table} WHERE AirplaneID = @AirplaneId";
        AirplaneModel? result = _connection.QueryFirstOrDefault<AirplaneModel>(sql, new { AirplaneId = airplaneID });
        return result;
    }

    public List<AirplaneModel> GetAirplanes()
    {
        string sql = $@"SELECT AirplaneID, AirplaneName FROM {Table}";
        List<AirplaneModel> result = _connection.Query<AirplaneModel>(sql).ToList();
        return result;
    }
}
