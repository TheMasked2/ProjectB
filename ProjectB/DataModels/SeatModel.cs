
public class SeatModel
{
    public string SeatID { get; set; }
    public string AirplaneID { get; set; }
    public int RowNumber { get; set; }
    public string ColumnLetter { get; set; }
    public string SeatClass { get; set; }
    public decimal Price { get; set; }
    public bool IsOccupied { get; set; }
    
    public SeatModel() 
    {
        IsOccupied = false;
    }

}