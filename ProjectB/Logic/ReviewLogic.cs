using Spectre.Console;
using ProjectB.DataAccess;
using Microsoft.VisualBasic;

public static class ReviewLogic
{
    public static IReviewAccess ReviewAccessService { get; set; } = new ReviewAcces();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static bool AddReview(ReviewModel review)
    {
        if (review.Rating < 1 || review.Rating > 5)
        {
            AnsiConsole.MarkupLine("[red]Rating must be between 1 and 5.[/]");
            return false;
        }
        if(FlightLogic.GetFlightById(review.FlightID) == null)
        {
            AnsiConsole.MarkupLine("[red]Flight does not exist.[/]");
            return false;
        }
        try
        {
            ReviewAcces reviewAccess = new ReviewAcces();
            reviewAccess.AddReview(review);
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error adding review: {ex.Message}[/]");
            return false;
        }
    }
    
    public static List<ReviewModel> GetAllReviews()
    {
        try
        {
            ReviewAcces reviewAccess = new ReviewAcces();
            return reviewAccess.GetAllReviews();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error retrieving reviews: {ex.Message}[/]");
            return new List<ReviewModel>();
        }
    }

        public static List<ReviewModel> FilterReviewsByFlightID(int flightid)
    {
        try
        {
            ReviewAcces reviewAccess = new ReviewAcces();
            return reviewAccess.GetReviewsByFlight(flightid);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error retrieving reviews: {ex.Message}[/]");
            return new List<ReviewModel>();
        }
    }
}