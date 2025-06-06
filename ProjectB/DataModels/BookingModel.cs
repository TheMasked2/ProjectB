public class BookingModel
{
    public int BookingID { get; set; }
    public int UserID { get; set; }
    public string PassengerName { get; set; }
    public int FlightID { get; set; }
    public string BookingDate { get; set; }
    public string BoardingTime { get; set; }
    public string SeatID { get; set; }
    public string SeatClass { get; set; }
    public string BookingStatus { get; set; }
    public string PaymentStatus { get; set; }
    public int AmountLuggage { get; set; } = 0; 
    public decimal TotalPrice { get; set; } = 0;
}