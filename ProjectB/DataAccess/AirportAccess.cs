using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public class AirportAccess : IAirportAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    public List<AirportModel> GetAllAirports()
    {
        string sql = "SELECT * FROM AIRPORTS ORDER BY City, Name";
        return _connection.Query<AirportModel>(sql).ToList();
    }

    public AirportModel GetAirportByCode(string iataCode)
    {
        string sql = "SELECT * FROM AIRPORTS WHERE IataCode = @IataCode";
        var parameters = new { IataCode = iataCode };
        return _connection.QueryFirstOrDefault<AirportModel>(sql, parameters);
    }
}