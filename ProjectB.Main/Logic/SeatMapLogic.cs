public static class SeatMapLogic
{
    // Get seat map for a specific flight (with occupancy)
    public static List<SeatModel> GetSeatMap(int flightId)
    {
        // Get seat info and occupancy for this flight
        var seatTuples = FlightSeatAccess.GetSeatsForFlight(flightId);
        // Set IsOccupied property from FlightSeats table
        foreach (var (seat, isOccupied) in seatTuples)
            seat.IsOccupied = isOccupied;
        return seatTuples.Select(t => t.seat).ToList();
    }

    // Returns seat letters and row numbers for a seat map
    public static (List<string> seatLetters, List<int> rowNumbers) GetSeatLayout(List<SeatModel> seats)
    {
        var seatLetters = seats.Select(s => s.SeatPosition).Distinct().OrderBy(c => c).ToList();
        var rowNumbers = seats.Select(s => s.RowNumber).Distinct().OrderBy(n => n).ToList();
        return (seatLetters, rowNumbers);
    }

    // Returns a seat if available, otherwise null
    public static SeatModel TryGetAvailableSeat(List<SeatModel> seats, int row, string seatLetter)
    {
        var seat = seats.FirstOrDefault(s => s.RowNumber == row && s.SeatPosition == seatLetter);
        if (seat != null && !seat.IsOccupied)
            return seat;
        return null;
    }

    // Mark seat as occupied for a specific flight
    public static void BookSeat(int flightId, SeatModel seat)
    {
        seat.IsOccupied = true;
        FlightSeatAccess.SetSeatOccupied(flightId, seat.SeatID, true);
    }
}