using ProjectB.DataAccess;

public static class ReviewLogic
{
    public static IReviewAccess ReviewAccessService { get; set; } = new ReviewAccess();

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
            ReviewAccessService.Insert(review);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Error adding review: {ex.Message}";
            return false;
        }
    }
    
    public static List<ReviewModel>? GetAllReviews(out string errorMessage)
    {
        errorMessage = null;
        try
        {
            if (ReviewAccessService.GetAll().Count == 0)
            {
                errorMessage = "Its quiet here, maybe a bit too quiet, no reviews yet.";
                return new List<ReviewModel>();
            }
            return ReviewAccessService.GetAll();
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