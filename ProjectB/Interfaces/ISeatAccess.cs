namespace ProjectB.DataAccess
{
    public interface ISeatAccess : IGenericAccess<SeatModel, string>
    {
        float GetSeatClassPrice(string airplaneID);
        float GetSeatClassPrice(string airplaneID, string seatClass);
    }
}