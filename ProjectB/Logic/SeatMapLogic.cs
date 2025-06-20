using ProjectB.DataAccess;

public static class SeatMapLogic
{
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();

    public static List<SeatModel> GetSeatMap(int flightId)
    {
        return FlightSeatAccessService.GetSeatsForFlight(flightId);
    }

    public static (List<string> seatLetters, List<int> rowNumbers) GetSeatLayout(List<SeatModel> seats)
    {
        var seatLetters = seats.Select(s => s.SeatPosition).Distinct().OrderBy(c => c).ToList();
        var rowNumbers = seats.Select(s => s.RowNumber).Distinct().OrderBy(n => n).ToList();
        return (seatLetters, rowNumbers);
    }

    public static List<string> BuildSeatMapLayout(List<SeatModel> seats) // Spectre.Console.Rendering.IRenderable if we want to refactor to table return <--
    {
        // Setup layout
        var (seatLettersRaw, rowNumbers) = GetSeatLayout(seats);
        List<string> seatLetters;
        int colCount = seatLettersRaw.Count;
        if (colCount == 2)
            seatLetters = new List<string> { "A" , "B" };
        else if (colCount == 4)
            seatLetters = new List<string> { "A", "B", "C", "D" };
        else if (colCount == 6)
            seatLetters = new List<string> { "A", "B", "C", "D", "E", "F" };
        else if (colCount == 8)
            seatLetters = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H" };
        else if (colCount == 10)
            seatLetters = new List<string> { "A", "B", "C", "D", "E", "F", "G" ,"H", "J", "K" };
        else 
            seatLetters = seatLettersRaw; // fallback to whatever is in the DB

        // Determine aisle positions based on column count and arrangement
        List<int> aisleAfter = new();
        if (colCount == 2) aisleAfter.Add(0); // A | B
        else if (colCount == 4) aisleAfter.Add(1); // A B | C D
        else if (colCount == 6) aisleAfter.Add(2); // A B C | D E F
        else if (colCount == 8) { aisleAfter.Add(2); aisleAfter.Add(4); } // A B C | D E | F G H
        else if (colCount == 10) { aisleAfter.Add(2); aisleAfter.Add(6); } // A B C | D E F G | H J K

        // Build the seat map
        var seatArt = new List<string>();

        // Header with aisle separation (4 spaces for each aisle)
        string header = "    ";
        for (int i = 0; i < seatLetters.Count; i++)
        {
            header += $" {seatLetters[i]} ";
            if (aisleAfter.Contains(i))
                header += "    ";
        }
        seatArt.Add(header);

        foreach (var row in rowNumbers)
        {
            string line = $"{row,3} ";
            for (int i = 0; i < seatLetters.Count; i++)
            {
                var letter = seatLetters[i];
                var seat = seats.FirstOrDefault(s => s.RowNumber == row && s.SeatPosition == letter);
                if (seat == null)
                {
                    line += "   ";
                }
                else if (seat.IsOccupied)
                {
                    line += "[red] X [/]";
                }
                else
                {
                    var seatType = (seat.SeatType ?? "").Trim().ToLower();
                    switch (seatType)
                    {
                        case "luxury":
                            line += "[yellow] L [/]";
                            break;
                        case "premium":
                            line += "[magenta] P [/]";
                            break;
                        case "standard extra legroom":
                            line += "[blue] E [/]";
                            break;
                        case "business":
                            line += "[cyan] B [/]";
                            break;
                        case "standard":
                            line += "[green] O [/]";
                            break;
                        default:
                            // Default to Unknown
                            line += "[grey] ? [/]";
                            break;
                    }
                }
                if (aisleAfter.Contains(i))
                    line += "    ";
            }
            seatArt.Add(line);
        }

        return seatArt;
    }

    public static SeatModel ValidateSeatInput(string seatInput, List<SeatModel> seats)
    {
        // Validate seat input format (e.g., 12A or A12)
        SeatModel? selectedSeat = null;
        string rowPart = "";
        string letterPart = "";

        // Try row+letter (e.g., 12A)
        var rowFirst = new string(seatInput.TakeWhile(char.IsDigit).ToArray());
        var letterAfter = new string(seatInput.SkipWhile(char.IsDigit).ToArray()).ToUpper();

        // Try letter+row (e.g., A12)
        var letterFirst = new string(seatInput.TakeWhile(char.IsLetter).ToArray()).ToUpper();
        var rowAfter = new string(seatInput.SkipWhile(char.IsLetter).ToArray());

        if (!string.IsNullOrEmpty(rowFirst) && !string.IsNullOrEmpty(letterAfter))
        {
            rowPart = rowFirst;
            letterPart = letterAfter; ;
        }
        else if (!string.IsNullOrEmpty(letterFirst) && !string.IsNullOrEmpty(rowAfter))
        {
            rowPart = rowAfter;
            letterPart = letterFirst;
        }
        else
        {
            return selectedSeat;
        }
        // Check if seat is occupied
        selectedSeat = TryGetAvailableSeat(seats, Convert.ToInt32(rowPart), letterPart);

        return selectedSeat;
    }

    public static SeatModel TryGetAvailableSeat(List<SeatModel> seats, int row, string seatLetter)
    {
        var seat = seats.FirstOrDefault(s => s.RowNumber == row && s.SeatPosition == seatLetter);
        if (seat != null && !seat.IsOccupied)
            return seat;
        return null;
    }

    public static void OccupySeat(int flightId, SeatModel seat)
    {
        seat.IsOccupied = true;
        FlightSeatAccessService.SetSeatOccupancy(flightId, seat.SeatID, true);
    }
}