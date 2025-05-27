namespace ProjectB.DataAccess
{
    public interface IBookingAccess
    {
        void AddBooking(BookingModel booking);
        List<BookingModel> GetBookingsByUser(int userId);
    }
}