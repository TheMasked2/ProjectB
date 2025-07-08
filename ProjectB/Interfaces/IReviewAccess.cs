namespace ProjectB.DataAccess
{
    public interface IReviewAccess : IGenericAccess<ReviewModel, int>
    {
        List<ReviewModel> GetReviewsByFlight(int flightId);
        List<ReviewModel> GetReviewsByUser(int userId);
        void DeleteReviewsByFlightID(int flightId);
    }

}