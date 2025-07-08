using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using ProjectB.DataAccess;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ReviewLogicUnitTests
    {
        [DataTestMethod]
        [DataRow(1, 101, "Great flight!", 5.0, true, DisplayName = "Valid review")]
        [DataRow(1, 101, "Bad rating", 0.0, false, DisplayName = "Rating too low")]
        [DataRow(1, 101, "Bad rating", 6.0, false, DisplayName = "Rating too high")]
        [DataRow(1, 9999, "Unknown flight", 4.0, false, DisplayName = "Non-existent flight")]
        public void AddReview_ValidatesAndAddsCorrectly(
            int userId,
            int flightId,
            string content,
            double rating,
            bool expectedResult)
        {
            // Arrange
            var mockReviewAccess = new Mock<IReviewAccess>();
            var mockFlightAccess = new Mock<IFlightAccess>();

            // Setup flight existence check
            if (flightId == 9999)
            {
                mockFlightAccess.Setup(x => x.GetById(flightId)).Returns((FlightModel?)null);
            }
            else
            {
                mockFlightAccess.Setup(x => x.GetById(flightId)).Returns(new FlightModel { FlightID = flightId });
            }

            ReviewLogic.ReviewAccessService = mockReviewAccess.Object;
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

            var review = new ReviewModel(userId, flightId, content, rating);

            // Act
            string errorMessage;
            var result = ReviewLogic.AddReview(review, out errorMessage);

            // Assert
            if (flightId == 9999 || rating < 1.0 || rating > 5.0)
            {
                Assert.IsFalse(result, errorMessage);
                Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
                mockReviewAccess.Verify(x => x.Insert(It.IsAny<ReviewModel>()), Times.Never);
            }
            else
            {
                Assert.IsTrue(result, errorMessage);
                Assert.IsTrue(string.IsNullOrEmpty(errorMessage));
                mockReviewAccess.Verify(x => x.Insert(review), Times.Once);
            }
        }

        [DataTestMethod]
        [DataRow(101, false, DisplayName = "Flight with reviews")]
        [DataRow(9999, true, DisplayName = "Flight with no reviews")]
        public void FilterReviewsByFlightID_ReturnsCorrectReviews(
            int flightId,
            bool expectError)
        {
            // Arrange
            var mockReviewAccess = new Mock<IReviewAccess>();
            var mockFlightAccess = new Mock<IFlightAccess>();

            // Setup flight existence check
            if (flightId == 9999)
            {
                mockFlightAccess.Setup(x => x.GetById(flightId)).Returns((FlightModel?)null);
            }
            else
            {
                mockFlightAccess.Setup(x => x.GetById(flightId)).Returns(new FlightModel { FlightID = flightId });
                mockReviewAccess.Setup(x => x.GetReviewsByFlight(flightId))
                    .Returns(new List<ReviewModel> { new ReviewModel(1, flightId, "Test", 5) });
            }

            ReviewLogic.ReviewAccessService = mockReviewAccess.Object;
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

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
            // Arrange
            var mockReviewAccess = new Mock<IReviewAccess>();
            mockReviewAccess.Setup(x => x.GetAll())
                .Returns(new List<ReviewModel>()); // Empty list to trigger error message

            ReviewLogic.ReviewAccessService = mockReviewAccess.Object;

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
}