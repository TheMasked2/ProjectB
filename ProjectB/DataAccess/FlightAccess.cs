using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class FlightAccess : GenericAccess<FlightModel, int>, IFlightAccess
{
    protected override string Table => "FLIGHTS";
    protected override string PrimaryKey => "FlightID";

    public override void Insert(FlightModel flight)
    {
        string sql = $@"INSERT INTO {Table}
                        (Airline, 
                        AirplaneID, 
                        DepartureAirport, 
                        ArrivalAirport, 
                        DepartureTime, 
                        ArrivalTime, 
                        Status) 
                        VALUES 
                        (@Airline, 
                        @AirplaneID, 
                        @DepartureAirport, 
                        @ArrivalAirport, 
                        @DepartureTime, 
                        @ArrivalTime,
                        @Status)";
        _connection.Execute(sql, flight);
    }

    public override void Update(FlightModel flight)
    {
        string sql = $@"UPDATE {Table} 
                        SET Airline = @Airline, 
                            AirplaneID = @AirplaneID, 
                            DepartureAirport = @DepartureAirport, 
                            ArrivalAirport = @ArrivalAirport, 
                            DepartureTime = @DepartureTime, 
                            ArrivalTime = @ArrivalTime,
                            Status = @Status 
                        WHERE FlightID = @FlightID";
        _connection.Execute(sql, flight);
    }

    public List<FlightModel> GetPastFlights(DateTime currentDate)
    {
        string sql = $@"SELECT * FROM {Table} 
                        WHERE DepartureTime < @CurrentTime";

        return _connection.Query<FlightModel>(sql, new { CurrentTime = currentDate }).ToList();
    }

    public List<FlightModel> GetUpcomingFlights(DateTime departingSoonDate)
    {
        string sql = $@"SELECT * FROM {Table} 
                        WHERE DepartureTime <= @SoonDate
                        AND Status != 'Departed'";

        return _connection.Query<FlightModel>(sql, new { SoonDate = departingSoonDate }).ToList();
    }

    public List<FlightModel> GetFilteredFlights(
        string? origin,
        string? destination,
        DateTime departureDate,
        bool past = false)
    {
        // Dynmamic SQL query for past or upcoming flights
        var sql = new System.Text.StringBuilder();
        sql.Append($@"SELECT * FROM {Table}
                        WHERE date(DepartureTime) = date(@DepartureDate)
                        AND DepartureAirport LIKE @Origin
                        AND ArrivalAirport LIKE @Destination");

        if (past)
        {
            sql.Append(" AND Status = 'Departed'");
        }
        else
        {
            sql.Append(" AND Status != 'Departed'");
        }

        var parameters = new
        {
            DepartureDate = departureDate,
            Origin = string.IsNullOrEmpty(origin) ? "%" : origin,
            Destination = string.IsNullOrEmpty(destination) ? "%" : destination
        };

        return _connection.Query<FlightModel>(sql.ToString(), parameters).ToList();
    }

    public void DeleteFlightsByIDs(List<int> flightIDs)
    {
        string sql = $@"DELETE FROM {Table} WHERE FlightID IN @FlightIDs";
        var parameters = new { FlightIDs = flightIDs };
        _connection.Execute(sql, parameters);
    }

    public List<int> GetOldDepartedFlightIDs(DateTime monthAgo)
    {
        string sql = $@"SELECT FlightID FROM {Table} 
                        WHERE DepartureTime < @MonthAgo 
                        AND Status = 'Departed'";
        var parameters = new { MonthAgo = monthAgo };
        return _connection.Query<int>(sql, parameters).ToList();
    }

    public int GetFlightIdByDetails(FlightModel flight)
    {
        string sql = $@"SELECT {PrimaryKey} FROM {Table} 
                        WHERE AirplaneID = @AirplaneID 
                        AND DepartureAirport = @DepartureAirport 
                        AND ArrivalAirport = @ArrivalAirport 
                        AND DepartureTime = @DepartureTime";
        
        return _connection.QuerySingle<int>(sql, flight);
    }
}