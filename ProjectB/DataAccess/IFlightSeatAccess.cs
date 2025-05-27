namespace ProjectB.DataAccess
{
    public interface IFlightSeatAccess
    {
        List<(SeatModel seat, bool isOccupied)> GetSeatsForFlight(int flightId);
        bool HasAnySeatsForFlight(int flightId);
        void BulkCreateAllFlightSeats(List<(int flightId, string airplaneId)> toBackfill);
        void SetSeatOccupied(int flightId, string seatId, bool isOccupied);
        void CreateFlightSeats(int flightId, string airplaneId);
    }
}