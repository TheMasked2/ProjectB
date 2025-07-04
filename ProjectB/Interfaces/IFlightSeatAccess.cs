namespace ProjectB.DataAccess
{
    public interface IFlightSeatAccess
    {
        List<SeatModel> GetSeatsForFlight(int flightId);
        bool HasAnySeatsForFlight(int flightId);
        void BulkCreateAllFlightSeats(List<(int flightId, string airplaneId)> toBackfill);
        void SetSeatOccupancy(int flightId, string seatId, bool isOccupied);
        void CreateFlightSeats(int flightId, string airplaneId);
        void DeleteFlightSeatsByFlightIDs(List<int> flightIDs);
        void DeleteFlightSeatsByFlightID(int flightId);
        int GetAvailableSeatCountByClass(int flightId, string airplaneId, string seatClass);
    }
}