using Spectre.Console;
using ProjectB.DataAccess;
using Microsoft.VisualBasic;

public static class ReviewLogic
{
    public static IReviewAccess ReviewAccessService { get; set; } = new ReviewAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));



    public static bool AddReview(ReviewModel review, out string errorMessage)
    {
        errorMessage = null;

        if (review.Rating < 1 || review.Rating > 5)
        {
            errorMessage = "Rating must be between 1 and 5.";
            return false;
        }
        if (FlightLogic.GetFlightById(review.FlightID) == null)
        {
            errorMessage = "Flight does not exist.";
            return false;
        }
        try
        {
            ReviewAccessService.AddReview(review);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Error adding review: {ex.Message}";
            return false;
        }
    }
    
    public static List<ReviewModel> GetAllReviews(out string errorMessage)
    {
        errorMessage = null;
        try
        {
            if (ReviewAccessService.GetAllReviews().Count == 0)
            {
                errorMessage = "Its quiet here, maybe a bit too quiet, no reviews yet.";
                return new List<ReviewModel>();
            }
            return ReviewAccessService.GetAllReviews();
        }
        catch (Exception ex)
        {
            errorMessage = $"Error retrieving reviews: {ex.Message}";
            return new List<ReviewModel>();
        }
    }

        public static List<ReviewModel> FilterReviewsByFlightID(int flightid, out string errorMessage)
    {
        errorMessage = null;
        try
        {
            if (FlightLogic.GetFlightById(flightid) == null)
            {
                errorMessage = $"No reviews found with FlightID: {flightid}";
                return new List<ReviewModel>();
            }
            return ReviewAccessService.GetReviewsByFlight(flightid);
        }
        catch (Exception ex)
        {
            errorMessage = $"Error retrieving reviews: {ex.Message}";
            return new List<ReviewModel>();
        }
    }
}