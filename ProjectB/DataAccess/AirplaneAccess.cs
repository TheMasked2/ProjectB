using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class AirplaneAccess : GenericAccess<AirplaneModel, string>, IAirplaneAccess
{
    protected override string Table => "AIRPLANE";
    protected override string PrimaryKey => "AirplaneID";

    public override void Insert(AirplaneModel airplane)
    {
        string sql = $@"INSERT INTO {Table} (AirplaneID, AirplaneName) 
                        VALUES (@AirplaneID, @AirplaneName)";
        _connection.Execute(sql, airplane);
    }

    public override void Update(AirplaneModel airplane)
    {
        string sql = $@"UPDATE {Table} 
                        SET AirplaneName = @AirplaneName 
                        WHERE AirplaneID = @AirplaneID";
        _connection.Execute(sql, airplane);
    }
}
