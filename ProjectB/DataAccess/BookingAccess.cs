using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public class BookingAccess : IBookingAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private static string Table = "BOOKINGS";

    public void AddBooking(BookingModel booking)
    {
        string sql = $@"INSERT INTO {Table} 
            (UserID, PassengerFirstName, PassengerLastName, PassengerEmail, PassengerPhone,
            FlightID, Airline, AirplaneModel, DepartureAirport, ArrivalAirport,
            DepartureTime, ArrivalTime, SeatID, SeatClass, LuggageAmount, HasInsurance,
            Discount, TotalPrice)
            VALUES (@UserID, @PassengerFirstName, @PassengerLastName, @PassengerEmail, @PassengerPhone,
            @FlightID, @Airline, @AirplaneModel, @DepartureAirport, @ArrivalAirport,
            @DepartureTime, @ArrivalTime, @SeatID, @SeatClass, @LuggageAmount, @HasInsurance,
            @Discount, @TotalPrice)";
        _connection.Execute(sql, booking);
    }

    public List<BookingModel> GetBookingsByUser(int userId)
    {
        string sql = $@"SELECT * FROM {Table} WHERE UserID = @UserID";
        return _connection.Query<BookingModel>(sql, new { UserID = userId }).ToList();
    }
    public BookingModel GetBookingById(int bookingId)
    {
        string sql = $@"SELECT * FROM {Table} WHERE BookingID = @BookingID";
        return _connection.QueryFirstOrDefault<BookingModel>(sql, new { BookingID = bookingId });
    }

    public void UpdateBooking(BookingModel booking)
    {
        string sql = $@"UPDATE {Table} 
                        SET PassengerFirstName = @PassengerFirstName,
                            PassengerLastName = @PassengerLastName,
                            PassengerEmail = @PassengerEmail,
                            PassengerPhone = @PassengerPhone,
                            SeatID = @SeatID,
                            SeatClass = @SeatClass,
                            LuggageAmount = @LuggageAmount,
                            HasInsurance = @HasInsurance,
                            Discount = @Discount,
                            TotalPrice = @TotalPrice
                        WHERE BookingID = @BookingID";
        _connection.Execute(sql, booking);
    }

}