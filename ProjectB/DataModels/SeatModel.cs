
public class SeatModel
{
    public string SeatID { get; set; }
    public string AirplaneID { get; set; }
    public int RowNumber { get; set; }
    public string SeatPosition { get; set; }
    public string SeatType { get; set; }
    public float Price { get; set; }
    public bool IsOccupied { get; set; }
    public AirplaneModel Airplane { get; set; }
    
    public SeatModel() 
    {
        IsOccupied = false;
    }

}