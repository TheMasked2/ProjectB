using System.Runtime.InteropServices;

public class SeatModel
{
    // Properties matching the database columns
    public string SeatID { get; set; }
    public string AirplaneID { get; set; }
    public int RowNumber { get; set; }
    public string SeatPosition { get; set; }
    public string SeatType { get; set; }
    public float Price { get; set; }
    public bool IsOccupied { get; set; }
    
    // Navigation property - not stored directly in database
    public AirplaneModel Airplane { get; set; }
    
    // Default constructor
    public SeatModel() 
    {
        IsOccupied = false;
    }

    // Parameterized constructor
    public SeatModel(string airplaneID, int rowNumber, string seatType, string seatLetter, float price, bool isOccupied = false)
    {
        AirplaneID = airplaneID;
        RowNumber = rowNumber;
        SeatPosition = seatLetter;
        SeatType = seatType;
        Price = price;
        IsOccupied = isOccupied;
        SeatID = $"{airplaneID}-{rowNumber}{seatLetter}";
    }

}