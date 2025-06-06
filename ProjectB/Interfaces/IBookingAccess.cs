namespace ProjectB.DataAccess
{
    public interface IBookingAccess
    {
        void AddBooking(BookingModel booking);
        List<BookingModel> GetBookingsByUser(int userId);
        BookingModel GetBookingById(int bookingId);
        void UpdateBooking(BookingModel booking);
    }
}