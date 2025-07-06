using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class SeatAccess : GenericAccess<SeatModel, string>, ISeatAccess
{
    protected override string Table => "SEATS";
    protected override string PrimaryKey => "SeatID";
    public float GetSeatClassPrice(string airplaneID)
    {

        string sql = $@"SELECT Price FROM {Table} WHERE AirplaneID = @AirplaneID";
        return _connection.QueryFirstOrDefault<float>(sql, new { AirplaneID = airplaneID});
    }

    public float GetSeatClassPrice(string airplaneID, string seatClass)
    {

        string sql = $@"SELECT Price FROM {Table} WHERE AirplaneID = @AirplaneID AND SeatClass = @SeatClass";
        return _connection.QueryFirstOrDefault<float>(sql, new { AirplaneID = airplaneID, SeatClass = seatClass });
    }

}