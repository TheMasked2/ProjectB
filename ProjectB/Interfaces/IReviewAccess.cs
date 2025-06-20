namespace ProjectB.DataAccess
{
public interface IReviewAccess
{
    void AddReview(ReviewModel review);
    List<ReviewModel> GetReviewsByFlight(int flightId);
    List<ReviewModel> GetAllReviews();
    List<ReviewModel> GetReviewsByUser(int userId);
}
}