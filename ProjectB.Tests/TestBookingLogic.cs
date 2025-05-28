using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectB.DataAccess; // Assuming BookingModel, User, FlightModel, SeatModel are here or accessible
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class BookingLogicTests
    {
        /// <summary>
        /// Tests BookingLogic.BackfillFlightSeats.
        /// Verifies that FlightSeatAccessService.BulkCreateAllFlightSeats is called correctly 
        /// based on whether flights need backfilling.
        /// </summary>
        [DataTestMethod]
        [DataRow(0, 0, false, 0, DisplayName = "No flights, no backfill")]
        [DataRow(2, 0, false, 0, DisplayName = "All flights have seats, no backfill")]
        [DataRow(3, 2, true, 2, DisplayName = "Some flights need backfill")]
        [DataRow(3, 3, true, 3, DisplayName = "All flights need backfill")]
        public void BackfillFlightSeats_CallsBulkCreate_AsExpected(int totalFlights, int flightsNeedingBackfill, bool expectCallToBulkCreate, int expectedBackfillCount)
        {
            // Arrange
            var mockFlightAccess = new Mock<IFlightAccess>(); 
            var mockFlightSeatAccess = new Mock<IFlightSeatAccess>(); 

            BookingLogic.FlightAccessService = mockFlightAccess.Object; 
            BookingLogic.FlightSeatAccessService = mockFlightSeatAccess.Object; 

            var flights = new List<FlightModel>();
            for (int i = 1; i <= totalFlights; i++)
            {
                flights.Add(new FlightModel { FlightID = i, AirplaneID = $"Airplane_{i}" });
            }
            mockFlightAccess.Setup(s => s.GetAllFlightData()).Returns(flights);

            var flightsThatNeedBackfillIds = flights.Take(flightsNeedingBackfill).Select(f => f.FlightID).ToList();

            mockFlightSeatAccess.Setup(s => s.HasAnySeatsForFlight(It.IsAny<int>()))
                .Returns<int>(flightId => !flightsThatNeedBackfillIds.Contains(flightId));

            List<(int, string)> capturedBackfillList = null;
            mockFlightSeatAccess.Setup(s => s.BulkCreateAllFlightSeats(It.IsAny<List<(int, string)>>()))
                                 .Callback<List<(int, string)>>(list => capturedBackfillList = list);

            // Act
            BookingLogic.BackfillFlightSeats();

            // Assert
            if (expectCallToBulkCreate)
            {
                mockFlightSeatAccess.Verify(s => s.BulkCreateAllFlightSeats(It.IsAny<List<(int, string)>>()), Times.Once);
                Assert.IsNotNull(capturedBackfillList, "Captured backfill list should not be null when BulkCreateAllFlightSeats is expected to be called.");
                Assert.AreEqual(expectedBackfillCount, capturedBackfillList.Count, "The number of flights in the backfill list is incorrect.");
                foreach (var flightId in flightsThatNeedBackfillIds)
                {
                    Assert.IsTrue(capturedBackfillList.Any(item => item.Item1 == flightId), $"Flight ID {flightId} was expected in the backfill list but not found.");
                }
            }
            else
            {
                mockFlightSeatAccess.Verify(s => s.BulkCreateAllFlightSeats(It.IsAny<List<(int, string)>>()), Times.Never);
            }
        }

        /// <summary>
        /// Tests if BookingLogic.CreateBooking correctly prepares a BookingModel
        /// and calls BookingAccessService.AddBooking with the correct details,
        /// reflecting the string format of SeatID.
        /// </summary>
        [TestMethod]
        public void CreateBooking_ConstructsModelAndCallsService_WithCorrectDetails()
        {
            // Arrange
            var mockBookingAccess = new Mock<IBookingAccess>(); 
            BookingLogic.BookingAccessService = mockBookingAccess.Object; 

            var user = new User { UserID = 1, FirstName = "John", LastName = "Doe" };
            var flight = new FlightModel { FlightID = 101, DepartureTime = new DateTime(2025, 12, 25, 10, 30, 0) };
            // Updated SeatModel instantiation based on your description
            var seat = new SeatModel { 
                SeatID = "XC101-8C",
                SeatType = "Economy",
                Price = 150,
                IsOccupied = false 
            };

            BookingModel capturedBookingModel = null;
            mockBookingAccess.Setup(service => service.AddBooking(It.IsAny<BookingModel>()))
                             .Callback<BookingModel>(bm => capturedBookingModel = bm);

            // Act
            BookingLogic.CreateBooking(user, flight, seat);

            // Assert
            mockBookingAccess.Verify(service => service.AddBooking(It.IsAny<BookingModel>()), Times.Once);

            Assert.IsNotNull(capturedBookingModel, "BookingModel passed to AddBooking should not be null.");
            Assert.AreEqual(user.UserID, capturedBookingModel.UserID);
            Assert.AreEqual($"{user.FirstName} {user.LastName}", capturedBookingModel.PassengerName);
            Assert.AreEqual(flight.FlightID, capturedBookingModel.FlightID);
            Assert.AreEqual(seat.SeatID, capturedBookingModel.SeatID, "SeatID in BookingModel should match the string SeatID from SeatModel.");
            Assert.AreEqual(seat.SeatType, capturedBookingModel.SeatClass, "SeatClass in BookingModel should match SeatType from SeatModel.");
            Assert.AreEqual("Confirmed", capturedBookingModel.BookingStatus);
            Assert.AreEqual("Paid", capturedBookingModel.PaymentStatus);
            Assert.AreEqual(flight.DepartureTime.ToString("yyyy-MM-dd HH:mm:ss"), capturedBookingModel.BoardingTime);
            
            Assert.IsTrue(DateTime.TryParse(capturedBookingModel.BookingDate, out DateTime bookingDateParsed));
            Assert.IsTrue((DateTime.Now - bookingDateParsed).TotalSeconds < 1, "BookingDate should be very close to the current time.");
        }

        /// <summary>
        /// Tests BookingLogic.GetBookingsForUser filters correctly for upcoming or past bookings
        /// and returns the expected bookings.
        /// </summary>
        [DataTestMethod]
        [DataRow(true, 2, new[] { 1, 2 }, DisplayName = "Upcoming bookings")]
        [DataRow(false, 2, new[] { 3, 4 }, DisplayName = "Past bookings")]
        public void GetBookingsForUser_FiltersByUpcomingOrPast_Correctly(bool upcoming, int expectedCount, int[] expectedBookingIds)
        {
            // Arrange
            var mockBookingAccess = new Mock<IBookingAccess>(); 
            BookingLogic.BookingAccessService = mockBookingAccess.Object; 

            int userId = 1;
            var now = DateTime.Now; 
            var allUserBookings = new List<BookingModel>
            {
                new BookingModel { UserID = userId, BookingID = 1, BoardingTime = now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss") }, 
                new BookingModel { UserID = userId, BookingID = 2, BoardingTime = now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") },
                new BookingModel { UserID = userId, BookingID = 3, BoardingTime = now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") },
                new BookingModel { UserID = userId, BookingID = 4, BoardingTime = now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss") } 
            };
            
            mockBookingAccess.Setup(s => s.GetBookingsByUser(userId)).Returns(allUserBookings);

            // Act
            var result = BookingLogic.GetBookingsForUser(userId, upcoming);

            // Assert
            Assert.AreEqual(expectedCount, result.Count, $"Expected {expectedCount} bookings but got {result.Count}.");
            foreach (var id in expectedBookingIds)
            {
                Assert.IsTrue(result.Any(b => b.BookingID == id), $"Booking with ID {id} was expected but not found.");
            }
            if (upcoming)
            {
                Assert.IsTrue(result.All(b => DateTime.Parse(b.BoardingTime) >= now), "Not all returned bookings are upcoming.");
            }
            else
            {
                Assert.IsTrue(result.All(b => DateTime.Parse(b.BoardingTime) < now), "Not all returned bookings are past.");
            }
        }

        /// <summary>
        /// Tests BookingLogic.GetBookingsForUser returns an empty list if the 
        /// BookingAccessService returns no bookings for the user.
        /// </summary>
        [TestMethod]
        public void GetBookingsForUser_ReturnsEmptyList_WhenNoBookingsExistForUser()
        {
            // Arrange
            var mockBookingAccess = new Mock<IBookingAccess>(); 
            BookingLogic.BookingAccessService = mockBookingAccess.Object; 

            int userId = 99; 
            mockBookingAccess.Setup(s => s.GetBookingsByUser(userId)).Returns(new List<BookingModel>());

            // Act
            var resultUpcoming = BookingLogic.GetBookingsForUser(userId, true);
            var resultPast = BookingLogic.GetBookingsForUser(userId, false);

            // Assert
            Assert.AreEqual(0, resultUpcoming.Count, "Upcoming bookings list should be empty.");
            Assert.AreEqual(0, resultPast.Count, "Past bookings list should be empty.");
        }

        /// <summary>
        /// Tests BookingLogic.GetNextBookingId returns the correct next available BookingID.
        /// </summary>
        [DataTestMethod]
        [DataRow(new int[] { 1, 2, 5 }, 6, DisplayName = "Max ID is 5, next is 6")] 
        [DataRow(new int[] { 10 }, 11, DisplayName = "Single booking, next is 11")]     
        [DataRow(new int[] { }, 1, DisplayName = "No bookings, next is 1")]         
        public void GetNextBookingId_ReturnsCorrectNextId(int[] existingBookingIds, int expectedNextId)
        {
            // Arrange
            var mockBookingAccess = new Mock<IBookingAccess>(); 
            BookingLogic.BookingAccessService = mockBookingAccess.Object; 

            var allBookings = existingBookingIds.Select(id => new BookingModel { BookingID = id }).ToList();
            mockBookingAccess.Setup(s => s.GetBookingsByUser(0)).Returns(allBookings);

            // Act
            int result = BookingLogic.GetNextBookingId();

            // Assert
            Assert.AreEqual(expectedNextId, result);
        }
    }
}