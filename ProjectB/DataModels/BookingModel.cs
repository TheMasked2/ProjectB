public class BookingModel
{
    public int BookingID { get; set; }
    public string? BookingStatus { get; set; }
    public int UserID { get; set; }
    public int FlightID { get; set; }
    public string? SeatID { get; set; }
    public int LuggageAmount { get; set; } = 0;
    public bool HasInsurance { get; set; } = false;
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}