using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public class BookingAccess : GenericAccess<BookingModel, int>, IBookingAccess
{
    protected override string Table => "BOOKINGS";
    protected override string PrimaryKey => "BookingID";

    public override void Insert(BookingModel booking)
    {
        string sql = $@"INSERT INTO {Table} 
                        (UserID, 
                        FlightID, 
                        BookingStatus, 
                        PassengerFirstName, 
                        PassengerLastName, 
                        PassengerEmail, 
                        PassengerPhone, 
                        SeatID, 
                        SeatClass, 
                        LuggageAmount, 
                        HasInsurance, 
                        Discount, 
                        TotalPrice) 
                        VALUES 
                        (@UserID, 
                        @FlightID, 
                        @BookingStatus, 
                        @PassengerFirstName, 
                        @PassengerLastName, 
                        @PassengerEmail, 
                        @PassengerPhone, 
                        @SeatID, 
                        @SeatClass, 
                        @LuggageAmount, 
                        @HasInsurance,
                        @Discount,
                        @TotalPrice)";
        _connection.Execute(sql, booking);
    }

    // TODO: Change bookingmodel to inherit info from user
    public override void Update(BookingModel booking)
    {
        string sql = $@"UPDATE {Table} 
                        SET BookingStatus = @BookingStatus,
                            PassengerFirstName = @PassengerFirstName,
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

    public List<BookingModel> GetBookingsByUser(int userId)
    {
        string sql = $@"SELECT * FROM {Table} WHERE UserID = @UserID";
        return _connection.Query<BookingModel>(sql, new { UserID = userId }).ToList();
    }
}