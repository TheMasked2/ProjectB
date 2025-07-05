public class ReviewModel
{
    public int ReviewID { get; set; } // Primary Key
    public int UserID { get; set; }
    public int FlightID { get; set; }
    public string Content { get; set; }
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; }

    public ReviewModel(int userID, int flightID, string content, double rating)
    {
        UserID = userID;
        FlightID = flightID;
        Content = content;
        Rating = rating;
        CreatedAt = DateTime.Now;
    }
}