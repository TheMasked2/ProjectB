using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public class AirportAccess : GenericAccess<AirportModel, string>, IAirportAccess
{
    protected override string Table => "AIRPORTS";
    protected override string PrimaryKey => "IataCode";
    public override void Insert(AirportModel airport)
    {
        string sql = $@"INSERT INTO {Table} 
                        (IataCode, 
                        Name, 
                        City, 
                        Country) 
                        VALUES 
                        (@IataCode, 
                        @Name, 
                        @City, 
                        @Country)";
        _connection.Execute(sql, airport);
    }

    public override void Update(AirportModel airport)
    {
        string sql = $@"UPDATE {Table} 
                        SET Name = @Name, 
                            City = @City, 
                            Country = @Country 
                        WHERE IataCode = @IataCode";
        _connection.Execute(sql, airport);
    }

}