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
            BookingLogic.CreateBooking(user, flight, seat, 0); // Add default amountLuggage parameter

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
        /// Tests if BookingLogic.CreateBooking correctly prepares a BookingModel
        /// and calls BookingAccessService.AddBooking with the correct details,
        /// reflecting the string format of SeatID and amountLuggage.
        /// </summary>
        [TestMethod]
        public void CreateBooking_ConstructsModelAndCallsService_WithCorrectDetails_AndLuggage()
        {
            // Arrange
            var mockBookingAccess = new Mock<IBookingAccess>();
            BookingLogic.BookingAccessService = mockBookingAccess.Object;

            var user = new User { UserID = 1, FirstName = "John", LastName = "Doe" };
            var flight = new FlightModel { FlightID = 101, DepartureTime = new DateTime(2025, 12, 25, 10, 30, 0) };
            var seat = new SeatModel {
                SeatID = "XC101-8C",
                SeatType = "Economy",
                Price = 150,
                IsOccupied = false
            };
            int luggage = 2;

            BookingModel capturedBookingModel = null;
            mockBookingAccess.Setup(service => service.AddBooking(It.IsAny<BookingModel>()))
                             .Callback<BookingModel>(bm => capturedBookingModel = bm);

            // Act
            BookingLogic.CreateBooking(user, flight, seat, luggage);

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
            Assert.AreEqual(luggage, capturedBookingModel.AmountLuggage, "AmountLuggage should match the provided value.");
            
            Assert.IsTrue(DateTime.TryParse(capturedBookingModel.BookingDate, out DateTime bookingDateParsed));
            Assert.IsTrue((DateTime.Now - bookingDateParsed).TotalSeconds < 1, "BookingDate should be very close to the current time.");
        }

        /// <summary>
        /// Tests if BookingLogic handles senior discount correctly
        /// </summary>
        [TestMethod]
        public void CreateBooking_AppliesSeniorDiscount_WhenUserIsOver65()
        {
            // Arrange
            var mockBookingAccess = new Mock<IBookingAccess>();
            BookingLogic.BookingAccessService = mockBookingAccess.Object;

            var user = new User { 
                UserID = 1, 
                FirstName = "John", 
                LastName = "Doe",
                BirthDate = DateTime.Now.AddYears(-66),
                FirstTimeDiscount = false  // Make sure first time discount is false
            };
            var flight = new FlightModel { FlightID = 101 };
            var seat = new SeatModel { 
                SeatID = "A1",
                SeatType = "Economy",
                Price = 100
            };

            BookingModel capturedBookingModel = null;
            mockBookingAccess.Setup(service => service.AddBooking(It.IsAny<BookingModel>()))
                             .Callback<BookingModel>(bm => capturedBookingModel = bm);

            // Act
            BookingLogic.CreateBooking(user, flight, seat, 0);

            // Assert
            Assert.IsNotNull(capturedBookingModel);
            Assert.AreEqual(80m, capturedBookingModel.TotalPrice); // 100 * 0.8 (20% senior discount)
        }

        [TestMethod]
        public void CalculateBookingPrice_WithFirstTimeDiscount_AppliesTenPercentDiscount()
        {
            // Arrange
            var user = new User { 
                UserID = 1, 
                FirstName = "John", 
                LastName = "Doe",
                FirstTimeDiscount = true
            };
            var flight = new FlightModel { FlightID = 101 };
            var seat = new SeatModel { 
                SeatID = "A1",
                SeatType = "Economy",
                Price = 100
            };
            int luggage = 1;

            // Act
            decimal price = BookingLogic.CalculateBookingPrice(user, seat, luggage);

            // Assert
            // Base price 100 + luggage 500, then 10% discount
            Assert.AreEqual(540m, price); // (100 + 500) * 0.9
        }

        [TestMethod]
        public void CalculateBookingPrice_WithSeniorDiscount_AppliesTwentyPercentDiscount()
        {
            // Arrange
            var user = new User { 
                UserID = 1, 
                FirstName = "John", 
                LastName = "Doe",
                BirthDate = DateTime.Now.AddYears(-66),
                FirstTimeDiscount = false
            };
            var flight = new FlightModel { FlightID = 101 };
            var seat = new SeatModel { 
                SeatID = "A1",
                SeatType = "Economy",
                Price = 100
            };
            int luggage = 1;

            // Act
            decimal price = BookingLogic.CalculateBookingPrice(user, seat, luggage);

            // Assert
            // Base price 100 + luggage 500, then 20% discount
            Assert.AreEqual(480m, price); // (100 + 500) * 0.8
        }

        [TestMethod]
        public void CalculateBookingPrice_WithLuggage_AddsLuggagePrice()
        {
            // Arrange
            var user = new User { 
                UserID = 1, 
                FirstName = "John", 
                LastName = "Doe",
                FirstTimeDiscount = false,
                BirthDate = DateTime.Now.AddYears(-30) // Make sure user isn't eligible for senior discount
            };
            var seat = new SeatModel { 
                SeatID = "A1",
                SeatType = "Economy",
                Price = 100
            };

            // Act
            decimal priceWithOneLuggage = BookingLogic.CalculateBookingPrice(user, seat, 1);
            decimal priceWithTwoLuggage = BookingLogic.CalculateBookingPrice(user, seat, 2);

            // Assert
            Assert.AreEqual(600m, priceWithOneLuggage); // 100 + (500 * 1)
            Assert.AreEqual(1100m, priceWithTwoLuggage); // 100 + (500 * 2)
        }

        [TestMethod]
        public void ViewUserBookings_ReturnsCorrectBookings()
        {
            // Arrange
            var mockBookingAccess = new Mock<IBookingAccess>();
            BookingLogic.BookingAccessService = mockBookingAccess.Object;

            var now = DateTime.Now;
            var bookings = new List<BookingModel>
            {
                new BookingModel { 
                    BookingID = 1,
                    UserID = 1,
                    BoardingTime = now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss")
                },
                new BookingModel {
                    BookingID = 2,
                    UserID = 1,
                    BoardingTime = now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss")
                }
            };

            mockBookingAccess.Setup(x => x.GetBookingsByUser(1)).Returns(bookings);

            // Act
            var upcomingBookings = BookingLogic.GetBookingsForUser(1, true);
            var pastBookings = BookingLogic.GetBookingsForUser(1, false);

            // Assert
            Assert.AreEqual(1, upcomingBookings.Count);
            Assert.AreEqual(1, pastBookings.Count);
            Assert.AreEqual(1, upcomingBookings[0].BookingID);
            Assert.AreEqual(2, pastBookings[0].BookingID);
        }
    }
}