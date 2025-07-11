namespace ProjectB.DataAccess
{
    public interface IBookingAccess : IGenericAccess<BookingModel, int>
    {
        List<BookingModel> GetBookingsByUser(int userId);
        List<BookingModel> GetBookingsByFlightId(int flightId);
    }
}