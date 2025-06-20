using Spectre.Console;
using ProjectB.DataAccess;
using Microsoft.VisualBasic;

public static class ReviewLogic
{
    public static IReviewAccess ReviewAccessService { get; set; } = new ReviewAccess();

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
            ReviewAccessService.AddReview(review);
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
            return ReviewAccessService.GetAllReviews();
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
            return ReviewAccessService.GetReviewsByFlight(flightid);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error retrieving reviews: {ex.Message}[/]");
            return new List<ReviewModel>();
        }
    }
}