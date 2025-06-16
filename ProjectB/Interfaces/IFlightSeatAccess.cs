namespace ProjectB.DataAccess
{
    public interface IFlightSeatAccess
    {
        List<SeatModel> GetSeatsForFlight(int flightId);
        bool HasAnySeatsForFlight(int flightId);
        void BulkCreateAllFlightSeats(List<(int flightId, string airplaneId)> toBackfill);
        void SetSeatOccupancy(int flightId, string seatId, bool isOccupied);
        void CreateFlightSeats(int flightId, string airplaneId);
        void DeletePastFlightSeatsByFlightIDs(List<int> flightIDs);
        int GetAvailableSeatCountByClass(int flightId, string airplaneId, string seatClass);

    }
}