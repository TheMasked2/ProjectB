using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class FlightSeatAccess : GenericAccess<SeatModel, string>, IFlightSeatAccess
{
    protected override string Table => "FLIGHTSEATS";
    protected override string PrimaryKey => "SeatID";
    private static string SeatsTable => "SEATS";

    public List<SeatModel> GetSeatsForFlight(int flightId)
    {
        // This SQL query retrieves all seat information for a specific flight.
        // It combines data from two tables: FlightSeats (fs) and Seats (s).
        // FlightSeats contains flight-specific seat data,
        // while Seats contains the general seat information.
        // The INNER JOIN connects these tables using SeatID to get complete seat details.
        // Results are ordered by row number and seat position for logical display.
        string sql = $@"
            SELECT 
                s.{PrimaryKey},
                s.AirplaneID,
                s.RowNumber,
                s.ColumnLetter,
                s.SeatClass,
                s.Price,
                fs.IsOccupied
            FROM {Table} fs
            INNER JOIN {SeatsTable} s ON fs.{PrimaryKey} = s.{PrimaryKey}
            WHERE fs.FlightID = @FlightID
            ORDER BY s.RowNumber, s.ColumnLetter
        ";
        return _connection.Query<SeatModel>(sql, new { FlightID = flightId }).ToList();
    }

    public bool HasAnySeatsForFlight(int flightId)
    {
        string sql = $"SELECT 1 FROM {Table} WHERE FlightID = @FlightID LIMIT 1";
        return _connection.QueryFirstOrDefault<int?>(sql, new { FlightID = flightId }) != null;
    }

    public void SetSeatOccupancy(int flightId, string seatId, bool isOccupied)
    {
        string sql = $"UPDATE {Table} SET IsOccupied = @IsOccupied WHERE FlightID = @FlightID AND {PrimaryKey} = @{PrimaryKey}";
        _connection.Execute(sql, new { IsOccupied = isOccupied, FlightID = flightId, SeatID = seatId });
    }

    public void CreateFlightSeats(int flightId, string airplaneId)
    {
        // This SQL query creates flight seats for a specific flight.:
        // It SELECTS all the SeatIDs from the SEATS table that match the airplaneId.
        // For each {PrimaryKey} found, it INSERTS a new row into the FLIGHTSEATS table.
        // It uses the provided flightId and sets IsOccupied to 0 (unoccupied) for all new rows.
        string sql = $@"
            INSERT INTO {Table} (FlightID, {PrimaryKey}, IsOccupied)
            SELECT @FlightID, {PrimaryKey}, 0
            FROM {SeatsTable}
            WHERE AirplaneID = @AirplaneID";

        var parameters = new { FlightID = flightId, AirplaneID = airplaneId };
        _connection.Execute(sql, parameters);
    }

    public void DeleteFlightSeatsByFlightIDs(List<int> flightIDs)
    {
        string sql = $@"DELETE FROM {Table} WHERE FlightID IN @FlightIDs";
        var parameters = new { FlightIDs = flightIDs };
        _connection.Execute(sql, parameters);
    }

    public void DeleteFlightSeatsByFlightID(int flightId)
    {
        string sql = $@"DELETE FROM {Table} WHERE FlightID = @FlightID";
        var parameters = new { FlightID = flightId };
        _connection.Execute(sql, parameters);
    }

    public int GetAvailableSeatCountByClass(int flightID, string airplaneID, string seatClass)
    {
        // This SQL query counts avialable seats for a specific flight and seat class.
        // It uses the Table (fs) to find seats for the given flight,
        // and the SeatsTable (s) to filter by seat type (class).
        // It then filters by the given FlightID, SeatType (class),
        // and returns the count of unoccupied seats (IsOccupied = 0).
        string sql = $@"
            SELECT COUNT(fs.{PrimaryKey})
            FROM {Table} fs
            INNER JOIN {SeatsTable} s ON fs.{PrimaryKey} = s.{PrimaryKey} AND s.AirplaneID = @AirplaneID 
            WHERE fs.FlightID = @FlightID
              AND s.SeatClass = @SeatClass
              AND fs.IsOccupied = 0";
        var parameters = new { FlightID = flightID, AirplaneID = airplaneID, SeatClass = seatClass };

        return _connection.ExecuteScalar<int>(sql, parameters);
    }

    public override void Insert(SeatModel seat)
    {
        string sql = $@"INSERT INTO {Table} 
                        (FlightID, {PrimaryKey}, IsOccupied) 
                        VALUES 
                        (@FlightID, @{PrimaryKey}, @IsOccupied)";
        _connection.Execute(sql, seat);
    }

    public override void Update(SeatModel seat)
    {
        string sql = $@"UPDATE {Table} 
                        SET IsOccupied = @IsOccupied 
                        WHERE FlightID = @FlightID AND {PrimaryKey} = @{PrimaryKey}";
        _connection.Execute(sql, seat);
    }
}