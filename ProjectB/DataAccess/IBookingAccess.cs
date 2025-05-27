public interface IBookingAccess
{
    void AddBooking(BookingModel booking);
    List<BookingModel> GetBookingsByUser(int userId);
    List<BookingModel> GetAllBookings();
    void UpdateBooking(BookingModel booking);
    void DeleteBooking(int bookingId);
}