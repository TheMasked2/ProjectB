public class BookingModel
{
    public int BookingID { get; set; }
    public string BookingStatus { get; set; }
    public int UserID { get; set; }
    public string PassengerFirstName { get; set; }
    public string PassengerLastName { get; set; }
    public string PassengerEmail { get; set; }
    public string PassengerPhone { get; set; }
    public int FlightID { get; set; }
    public string Airline { get; set; }
    public string AirplaneModel { get; set; }
    public string DepartureAirport { get; set; }
    public string ArrivalAirport { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string SeatID { get; set; }
    public string SeatClass { get; set; }
    public int LuggageAmount { get; set; } = 0;
    public bool HasInsurance { get; set; } = false;
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}