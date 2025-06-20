public class ReviewModel
{
    public int UserID { get; set; }
    public int FlightID { get; set; }
    public string Content { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }

    public ReviewModel(int userID, int flightID, string content, int rating)
    {
        UserID = userID;
        FlightID = flightID;
        Content = content;
        Rating = rating;
        CreatedAt = DateTime.Now;
    }
}