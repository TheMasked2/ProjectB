namespace ProjectB.DataAccess
{
    public interface ISeatAccess
    {
        float GetSeatClassPrice(string airplaneID);
        float GetSeatClassPrice(string airplaneID, string seatClass);
    }
}