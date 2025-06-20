using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

[TestClass]
[DoNotParallelize]
public class ReviewLogicUnitTests
{
    [DataTestMethod]
    [DataRow(1, 101, "Great flight!", 5, true, DisplayName = "Valid review")]
    [DataRow(1, 101, "Bad rating", 0, false, DisplayName = "Rating too low")]
    [DataRow(1, 101, "Bad rating", 6, false, DisplayName = "Rating too high")]
    [DataRow(1, 9999, "Unknown flight", 4, false, DisplayName = "Non-existent flight")]
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
        string errorMessage;
        var result = ReviewLogic.AddReview(review, out errorMessage);

        // Assert
        Assert.AreEqual(expectedResult, result, errorMessage);
        if (!expectedResult)
        {
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
        }
    }

    [DataTestMethod]
    [DataRow(101, false, DisplayName = "Flight with reviews")]
    [DataRow(9999, true, DisplayName = "Flight with no reviews")]
    public void FilterReviewsByFlightID_ReturnsCorrectReviews(
        int flightId,
        bool expectError)
    {
        // Act
        string errorMessage;
        List<ReviewModel> reviews = ReviewLogic.FilterReviewsByFlightID(flightId, out errorMessage);

        // Assert
        if (expectError)
        {
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
            Assert.AreEqual(0, reviews.Count);
        }
        else
        {
            Assert.IsTrue(string.IsNullOrEmpty(errorMessage));
            Assert.IsTrue(reviews.Count >= 0);
        }
    }

    [TestMethod]
    public void GetAllReviews_ReturnsListOrError()
    {
        // Act
        string errorMessage;
        List<ReviewModel> reviews = ReviewLogic.GetAllReviews(out errorMessage);

        // Assert
        if (reviews.Count == 0)
        {
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
        }
        else
        {
            Assert.IsTrue(string.IsNullOrEmpty(errorMessage));
        }
    }
}