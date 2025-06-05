using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public class BookingAccess : IBookingAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    private static string Table = "Bookings";
    
    public void AddBooking(BookingModel booking)
    {
        string sql = $@"INSERT INTO {Table} 
            (PassengerName, FlightID, BookingDate, BoardingTime, SeatID, SeatClass, BookingStatus, PaymentStatus, UserID, AmountLuggage, InsuranceStatus)
            VALUES (@PassengerName, @FlightID, @BookingDate, @BoardingTime, @SeatID, @SeatClass, @BookingStatus, @PaymentStatus, @UserID, @AmountLuggage, @InsuranceStatus)";
        _connection.Execute(sql, booking);
    }

    public List<BookingModel> GetBookingsByUser(int userId)
    {
        string sql = $@"SELECT * FROM {Table} WHERE UserID = @UserID";
        return _connection.Query<BookingModel>(sql, new { UserID = userId }).ToList();
    }
}