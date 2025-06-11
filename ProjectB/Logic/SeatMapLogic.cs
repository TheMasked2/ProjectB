using ProjectB.DataAccess;

public class SeatMapLogic
{
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();

    public List<SeatModel> GetSeatMap(int flightId)
    {
        return FlightSeatAccessService.GetSeatsForFlight(flightId);
    }

    public (List<string> seatLetters, List<int> rowNumbers) GetSeatLayout(List<SeatModel> seats)
    {
        var seatLetters = seats.Select(s => s.SeatPosition).Distinct().OrderBy(c => c).ToList();
        var rowNumbers = seats.Select(s => s.RowNumber).Distinct().OrderBy(n => n).ToList();
        return (seatLetters, rowNumbers);
    }

    public SeatModel TryGetAvailableSeat(List<SeatModel> seats, int row, string seatLetter)
    {
        var seat = seats.FirstOrDefault(s => s.RowNumber == row && s.SeatPosition == seatLetter);
        if (seat != null && !seat.IsOccupied)
            return seat;
        return null;
    }

    public void BookSeat(int flightId, SeatModel seat)
    {
        seat.IsOccupied = true;
        FlightSeatAccessService.SetSeatOccupied(flightId, seat.SeatID, true);
    }
}