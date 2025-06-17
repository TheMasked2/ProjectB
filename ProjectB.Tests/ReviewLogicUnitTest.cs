[TestClass]
[DoNotParallelize]
public class ReviewLogicUnitTests
{
    [DataTestMethod]
    [DataRow(1, 101, "Great flight!", 5, true)]
    [DataRow(1, 0, "Good service", 0, false)]
    [DataRow(1, 101, "Amazing!", 6, false)]
    public void AddReview_ValidatesAndAddsCorrectly(
        int userId,
        int flightId,
        string content,
        int rating,
        bool expectedResult)
    {
        // Arrange
        var review = new ReviewModel(userId, flightId, content, rating);

        // Act
        var result = ReviewLogic.AddReview(review);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }
}