using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq; // Added for Linq operations
using ProjectB.DataAccess;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class BookingLogicUnitTest
    {
        private Mock<IBookingAccess> mockBookingAccess;
        private Mock<IFlightAccess> mockFlightAccess;
        private Mock<IFlightSeatAccess> mockFlightSeatAccess;
        private Mock<IAirplaneAccess> mockAirplaneAccess;

        [TestInitialize]
        public void Setup()
        {
            mockBookingAccess = new Mock<IBookingAccess>();
            mockFlightAccess = new Mock<IFlightAccess>();
            mockFlightSeatAccess = new Mock<IFlightSeatAccess>();
            mockAirplaneAccess = new Mock<IAirplaneAccess>();

            BookingLogic.BookingAccessService = mockBookingAccess.Object;
            BookingLogic.FlightAccessService = mockFlightAccess.Object;
            BookingLogic.FlightSeatAccessService = mockFlightSeatAccess.Object;
            AirplaneLogic.AirplaneAccessService = mockAirplaneAccess.Object;

            var user = new User
            {
                UserID = 1,
                Guest = false,
                FirstTimeDiscount = false,
                BirthDate = DateTime.Now.AddYears(-30) // A standard non-senior user
            };
            SessionManager.SetCurrentUser(user);
        }


        [DataTestMethod]
        [DataRow(false, true, -30, 0, false, false, false, 100.0, 1.0, 0.0)] // Basic price
        [DataRow(false, true, -30, 0, true, false, false, 120.0, 1.0, 20.0)] // With insurance
        [DataRow(false, true, -30, 1, false, false, false, 150.0, 1.0, 0.0)] // With 1 luggage
        [DataRow(false, true, -30, 2, false, false, false, 200.0, 1.0, 0.0)] // With 2 luggage
        [DataRow(true, true, -30, 0, false, false, false, 90.0, 0.9, 0.0)] // First-time discount
        [DataRow(false, false, -70, 0, false, false, false, 80.0, 0.8, 0.0)] // Senior discount
        [DataRow(false, true, -30, 0, false, true, false, 95.0, 0.95, 0.0)] // Coupon discount
        [DataRow(false, true, -30, 0, false, false, true, -100.0, 1.0, 0.0)] // Spice coupon
        [DataRow(true, false, -70, 1, true, true, false, 110.5, 0.65, 20.0)] // Multiple discounts
        [DataRow(false, true, -70, 0, false, false, false, 100.0, 1.0, 0.0)] // Guest senior (no discount)
        // Border cases for senior discount
        [DataRow(false, false, -64, 0, false, false, false, 100.0, 1.0, 0.0)] // Just below senior threshold (64 years)
        [DataRow(false, false, -65, 0, false, false, false, 80.0, 0.8, 0.0)] // Exactly at senior threshold (65 years)
        [DataRow(false, false, -66, 0, false, false, false, 80.0, 0.8, 0.0)] // Just above senior threshold (66 years)
        // Guest senior border cases (should never get discount regardless of age)
        [DataRow(false, true, -64, 0, false, false, false, 100.0, 1.0, 0.0)] // Guest just below senior threshold
        [DataRow(false, true, -65, 0, false, false, false, 100.0, 1.0, 0.0)] // Guest exactly at senior threshold
        [DataRow(false, true, -66, 0, false, false, false, 100.0, 1.0, 0.0)] // Guest just above senior threshold
        public void CalculateBookingPrice_ReturnsCorrectAmounts(
            bool firstTimeDiscount, bool isGuest, int ageYears,
            int luggage, bool insurance, bool hasCoupon, bool isSpice,
            double expectedPrice, double expectedDiscount, double expectedInsurance)
        {
            // Arrange
            User user = new User
            {
                FirstTimeDiscount = firstTimeDiscount,
                Guest = isGuest,
                BirthDate = DateTime.Now.AddYears(ageYears)
            };
            FlightModel flight = new FlightModel { FlightID = 1 };
            SeatModel seat = new SeatModel { SeatID = "1A", Price = 100.0f };
            var coupon = (hasCoupon, isSpice);

            // Act
            var result = BookingLogic.CalculateBookingPrice(user, flight, seat, luggage, insurance, coupon);

            // Assert
            Assert.AreEqual((decimal)expectedPrice, result.finalPrice, 0.01m, "Final price calculation is incorrect");
            Assert.AreEqual((decimal)expectedDiscount, result.discount, 0.01m, "Discount calculation is incorrect");
            Assert.AreEqual((decimal)expectedInsurance, result.insurancePrice, 0.01m, "Insurance price calculation is incorrect");
        }

        [DataTestMethod]
        [DataRow(123, "John", "Doe", "john@example.com", "555-1234", false, false, -30,
                    456, "Test Airlines", "Boeing 737", "AMS", "JFK", "2025-07-15 10:30", "2025-07-15 18:45",
                    "15A", "Business", 200.0,
                    false, false, 1, true,
                    "Pending", 290.0, 1.0)] // Regular booking with luggage and insurance
        [DataRow(456, "Jane", "Smith", "jane@example.com", "555-5678", true, false, -70,
                    789, "Test Airways", "Airbus A320", "LHR", "CDG", "2025-08-20 12:00", "2025-08-20 14:30",
                    "5C", "First Class", 500.0,
                    true, false, 2, true,
                    "Pending", 455.0, 0.65)] // Booking with all discounts, luggage and insurance
        [DataRow(789, "Spice", "User", "spice@dune.com", "123-4567", false, true, -25,
                    101, "Spice Airlines", "Spice Jet", "ARR", "SIC", "2025-09-10 14:00", "2025-09-10 16:30",
                    "7B", "Economy", 150.0,
                    false, true, 0, false,
                    "Pending", -150.0, 1.0)] // Spice coupon booking (negative price easter egg)
        public void BookingBuilder_CreatesCorrectBookingModels(
            // User parameters
            int userId, string firstName, string lastName, string email, string phone, bool firstTimeDiscount, bool isGuest, int ageYears,
            // Flight parameters
            int flightId, string airline, string airplaneId, string departureAirport, string arrivalAirport, string departureTimeStr, string arrivalTimeStr,
            // Seat parameters
            string seatId, string seatType, double seatPrice,
            // Booking options
            bool hasCoupon, bool isSpice, int luggageAmount, bool hasInsurance,
            // Expected results
            string expectedStatus, double expectedPrice, double expectedDiscount)
        {
            // Arrange
            User user = new User
            {
                UserID = userId,
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = email,
                PhoneNumber = phone,
                FirstTimeDiscount = firstTimeDiscount,
                Guest = isGuest,
                BirthDate = DateTime.Now.AddYears(ageYears)
            };

            DateTime departureTime = DateTime.Parse(departureTimeStr);
            DateTime arrivalTime = DateTime.Parse(arrivalTimeStr);

            FlightModel flight = new FlightModel
            {
                FlightID = flightId,
                Airline = airline,
                AirplaneID = airplaneId,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime
            };

            SeatModel seat = new SeatModel
            {
                SeatID = seatId,
                SeatType = seatType,
                Price = (float)seatPrice
            };

            var coupon = (hasCoupon, isSpice);

            // Act
            BookingModel result = BookingLogic.BookingBuilder(user, flight, seat, coupon, luggageAmount, hasInsurance);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedStatus, result.BookingStatus);
            Assert.AreEqual(userId, result.UserID);
            Assert.AreEqual(firstName, result.PassengerFirstName);
            Assert.AreEqual(lastName, result.PassengerLastName);
            Assert.AreEqual(email, result.PassengerEmail);
            Assert.AreEqual(phone, result.PassengerPhone);
            Assert.AreEqual(flightId, result.FlightID);
            Assert.AreEqual(airline, result.Airline);
            Assert.AreEqual(airplaneId, result.AirplaneModel);
            Assert.AreEqual(departureAirport, result.DepartureAirport);
            Assert.AreEqual(arrivalAirport, result.ArrivalAirport);
            Assert.AreEqual(departureTime, result.DepartureTime);
            Assert.AreEqual(arrivalTime, result.ArrivalTime);
            Assert.AreEqual(seatId, result.SeatID);
            Assert.AreEqual(seatType, result.SeatClass);
            Assert.AreEqual(luggageAmount, result.LuggageAmount);
            Assert.AreEqual(hasInsurance, result.HasInsurance);

            Assert.AreEqual((decimal)expectedDiscount, result.Discount, 0.01m);
            Assert.AreEqual((decimal)expectedPrice, result.TotalPrice, 0.01m);
        }

        [DataTestMethod]
        [DataRow(1, true, new[] { 1, 3 })]     // User 1, Upcoming bookings (IDs 1, 3)
        [DataRow(1, false, new[] { 2, 5, 8 })] // User 1, Past bookings (IDs 2, 5, 8)
        [DataRow(2, true, new[] { 4 })]        // User 2, Upcoming bookings (ID 4)
        [DataRow(2, false, new[] { 6 })]       // User 2, Past bookings (ID 6)
        [DataRow(3, true, new int[] { })]      // User 3, No upcoming bookings
        [DataRow(3, false, new[] { 7 })]       // User 3, Only past bookings (ID 7)
        public void GetBookingsForUser_ReturnsCorrectBookings(int userId, bool upcoming, int[] expectedBookingIds)
        {
            // Arrange
            DateTime now = DateTime.Now;
            DateTime yesterday = now.AddDays(-1);
            DateTime tomorrow = now.AddDays(1);

            // Make bookings
            List<BookingModel> allBookings = new List<BookingModel>
            {
                new BookingModel { BookingID = 1, UserID = 1, DepartureTime = tomorrow, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 2, UserID = 1, DepartureTime = yesterday, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 3, UserID = 1, DepartureTime = tomorrow.AddDays(3), BookingStatus = "Pending" },
                new BookingModel { BookingID = 4, UserID = 2, DepartureTime = tomorrow.AddDays(5), BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 5, UserID = 1, DepartureTime = yesterday.AddDays(-2), BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 6, UserID = 2, DepartureTime = yesterday.AddDays(-5), BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 7, UserID = 3, DepartureTime = yesterday.AddDays(-1), BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 8, UserID = 1, DepartureTime = yesterday, BookingStatus = "Cancelled" }   // Cancelled bookings only show in past
            };

            // Setup mock
            mockBookingAccess
                .Setup(mock => mock.GetBookingsByUser(It.IsAny<int>()))
                .Returns<int>((id) =>
                {
                    return allBookings
                        .Where(booking => booking.UserID == id)
                        .ToList();
                });

            BookingLogic.BookingAccessService = mockBookingAccess.Object;

            // Act
            List<BookingModel> result = BookingLogic.GetBookingsForUser(userId, upcoming);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(expectedBookingIds.Length, result.Count, "Number of bookings should match expected");

            foreach (BookingModel booking in result)
            {
                if (upcoming)
                {
                    Assert.IsTrue(booking.DepartureTime >= now && booking.BookingStatus != "Cancelled",
                        $"Booking {booking.BookingID} shouldn't be in upcoming list");
                }
                else
                {
                    Assert.IsTrue(booking.DepartureTime < now || booking.BookingStatus == "Cancelled",
                        $"Booking {booking.BookingID} shouldn't be in past list");
                }
            }
        }

        [DataTestMethod]
        // Scenario 1: No existing seats, airplane has enough seats -> seats should be created.
        [DataRow(1, "AP100", 5, false, true, DisplayName = "CreateSeats: No existing seats, 5 total seats in airplane")]
        // Scenario 2: Existing seats -> seats should NOT be created.
        [DataRow(2, "AP200", 5, true, false, DisplayName = "NoCreateSeats: Existing seats, 5 total seats in airplane")]
        // Scenario 3: No existing seats, but airplane has 1 seat -> seats should NOT be created (loop condition i < 1 is false).
        [DataRow(3, "AP300", 1, false, false, DisplayName = "NoCreateSeats: No existing seats, 1 total seat in airplane")]
        // Scenario 4: No existing seats, but airplane has 0 seats -> seats should NOT be created.
        [DataRow(4, "AP400", 0, false, false, DisplayName = "NoCreateSeats: No existing seats, 0 total seats in airplane")]
        public void BackfillFlightSeats_CreatesOrSkipsSeats_BasedOnExistingSeatsAndTotalSeats(
            int flightId, string airplaneId, int airplaneTotalSeats,
            bool hasExistingSeats, bool expectSeatCreation)
        {
            // Arrange
            var flightToReturn = new FlightModel { FlightID = flightId, AirplaneID = airplaneId };
            var airplaneToReturn = new AirplaneModel { AirplaneID = airplaneId, TotalSeats = airplaneTotalSeats };

            mockFlightAccess.Setup(service => service.GetById(flightId)).Returns(flightToReturn);
            mockAirplaneAccess.Setup(service => service.GetAirplaneByID(airplaneId)).Returns(airplaneToReturn);
            mockFlightSeatAccess.Setup(service => service.HasAnySeatsForFlight(flightId)).Returns(hasExistingSeats);

            // Act
            BookingLogic.BackfillFlightSeats(flightId);

            // Assert
            // Verify essential calls were made
            mockFlightAccess.Verify(service => service.GetById(flightId), Times.Once());
            mockAirplaneAccess.Verify(service => service.GetAirplaneByID(airplaneId), Times.Once());
            mockFlightSeatAccess.Verify(service => service.HasAnySeatsForFlight(flightId), Times.Once());

            if (expectSeatCreation)
            {
                Assert.IsTrue(airplaneTotalSeats > 1, "Precondition for seat creation: airplaneTotalSeats must be > 1.");
                mockFlightSeatAccess.Verify(service => service.CreateFlightSeats(flightId, airplaneId),
                    Times.Exactly(airplaneTotalSeats - 1),
                    $"CreateFlightSeats should be called {airplaneTotalSeats - 1} times.");
            }
            else
            {
                mockFlightSeatAccess.Verify(service => service.CreateFlightSeats(It.IsAny<int>(), It.IsAny<string>()),
                    Times.Never(),
                    "CreateFlightSeats should not be called.");
            }
        }


        [DataTestMethod]
        // Scenario 1: Booking not found
        [DataRow(1, false, false, 0.0, false, false, 0.0, DisplayName = "BookingNotFound")]
        // Scenario 2: Booking found, has insurance -> full refund
        [DataRow(2, true, true, 250.0, true, true, 0.0, DisplayName = "CancelWithInsurance_FullRefund")]
        // Scenario 3: Booking found, no insurance, price > 100 -> fee applied
        [DataRow(3, true, false, 250.0, true, false, 150.0, DisplayName = "CancelNoInsurance_FeeApplied_PriceReduced")]
        // Scenario 4: Booking found, no insurance, price <= 100 -> price becomes 0
        [DataRow(4, true, false, 50.0, true, false, 0.0, DisplayName = "CancelNoInsurance_PriceBecomesZero")]
        public void CancelBooking_HandlesCoreLogicAndReturnsCorrectStatus(
            int bookingIdToCancel,
            bool bookingShouldExist,
            bool initialHasInsurance,
            double initialTotalPrice,
            bool expectedSuccessResult,
            bool expectedFreeCancelResult,
            double expectedFinalBookingPrice)
        {
            // Arrange
            BookingModel foundBooking = null;
            BookingModel capturedUpdatedBooking = null;
            int flightIdForMockBooking = 0;
            string seatIdForMockBooking = null;

            if (bookingShouldExist)
            {
                flightIdForMockBooking = bookingIdToCancel + 1000;
                seatIdForMockBooking = $"S{bookingIdToCancel}";
                foundBooking = new BookingModel
                {
                    BookingID = bookingIdToCancel,
                    FlightID = flightIdForMockBooking,
                    SeatID = seatIdForMockBooking,
                    HasInsurance = initialHasInsurance,
                    TotalPrice = (decimal)initialTotalPrice,
                    BookingStatus = "Confirmed"
                };
            }

            mockBookingAccess.Setup(service => service.GetBookingById(bookingIdToCancel)).Returns(foundBooking);
            mockBookingAccess.Setup(service => service.UpdateBooking(It.IsAny<BookingModel>()))
                                     .Callback<BookingModel>(updatedBooking => capturedUpdatedBooking = updatedBooking);
            mockFlightSeatAccess.Setup(service => service.SetSeatOccupancy(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()));

            // Act
            (bool success, bool freeCancel) resultTuple = BookingLogic.CancelBooking(bookingIdToCancel);

            // Assert
            Assert.AreEqual(expectedSuccessResult, resultTuple.success, "The 'success' part of the returned tuple was incorrect.");
            Assert.AreEqual(expectedFreeCancelResult, resultTuple.freeCancel, "The 'freeCancel' part of the returned tuple was incorrect.");

            if (expectedSuccessResult)
            {
                // Verify that GetBookingById was called
                mockBookingAccess.Verify(service => service.GetBookingById(bookingIdToCancel), Times.Once);

                // Verify SetSeatOccupancy was called correctly
                Assert.IsNotNull(foundBooking, "FoundBooking should not be null for a successful cancellation.");
                mockFlightSeatAccess.Verify(service => service.SetSeatOccupancy(foundBooking.FlightID, foundBooking.SeatID, false), Times.Once,
                    "SetSeatOccupancy was not called with the correct parameters or not called once.");

                // Verify UpdateBooking was called
                mockBookingAccess.Verify(service => service.UpdateBooking(It.IsAny<BookingModel>()), Times.Once,
                    "UpdateBooking was not called once for a successful cancellation.");

                // Verify the details of the updated booking
                Assert.IsNotNull(capturedUpdatedBooking, "The booking passed to UpdateBooking was not captured.");
                Assert.AreEqual("Cancelled", capturedUpdatedBooking.BookingStatus, "BookingStatus was not updated to 'Cancelled'.");
                Assert.AreEqual((decimal)expectedFinalBookingPrice, capturedUpdatedBooking.TotalPrice, 0.01m, "The final TotalPrice of the booking is incorrect.");
                Assert.AreEqual(initialHasInsurance, capturedUpdatedBooking.HasInsurance, "HasInsurance flag should not change on cancellation.");
            }
            else // Booking not found or other failure handled by returning (false, false)
            {
                // Verify that GetBookingById was called
                mockBookingAccess.Verify(service => service.GetBookingById(bookingIdToCancel), Times.Once);

                // Verify that dependent services were not called if booking was not found
                mockFlightSeatAccess.Verify(service => service.SetSeatOccupancy(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never,
                    "SetSeatOccupancy should not have been called if the booking was not found.");
                mockBookingAccess.Verify(service => service.UpdateBooking(It.IsAny<BookingModel>()), Times.Never,
                    "UpdateBooking should not have been called if the booking was not found.");
            }
        }

        [DataTestMethod]
        [DataRow(123, "Pending", DisplayName = "ConfirmPendingBooking")] // BookingID, InitialStatus
        [DataRow(456, "SomeOtherStatus", DisplayName = "ConfirmOtherStatusBooking")] // To show it always sets to Confirmed
        public void BookTheDamnFlight_SetsStatusToConfirmedAndAddsBooking(int bookingId, string initialStatus)
        {
            // Arrange
            var initialBooking = new BookingModel
            {
                BookingID = bookingId,
                UserID = 1,
                PassengerFirstName = "Test",
                PassengerLastName = "User",
                FlightID = 100,
                SeatID = "1A",
                BookingStatus = initialStatus // Set an initial status
            };

            BookingModel capturedBooking = null;
            mockBookingAccess
                .Setup(service => service.AddBooking(It.IsAny<BookingModel>()))
                .Callback<BookingModel>(b => capturedBooking = b); // Capture the booking passed to AddBooking

            // Act
            BookingLogic.BookTheDamnFlight(initialBooking);

            // Assert
            // Verify that AddBooking was called exactly once
            mockBookingAccess.Verify(service => service.AddBooking(It.IsAny<BookingModel>()), Times.Once,
                "AddBooking should be called exactly once.");

            // Verify status is "Confirmed"
            Assert.IsNotNull(capturedBooking, "A booking should have been captured by the AddBooking mock.");
            Assert.AreEqual("Confirmed", capturedBooking.BookingStatus, "The booking status should be set to 'Confirmed' before being added.");
            Assert.AreEqual(initialBooking.BookingID, capturedBooking.BookingID, "BookingID should remain the same.");
            Assert.AreEqual(initialBooking.UserID, capturedBooking.UserID, "UserID should remain the same.");
        }

        [DataTestMethod]
        // Arguments: newSeatId, newSeatClass, newSeatPrice, expectedTotalPrice
        // The expected total price is the new seat price + a fixed luggage cost of $50.
        [DataRow("2B", "Business", 200.0f, 250.0, DisplayName = "ModifyBooking: More expensive seat")]
        [DataRow("5C", "Economy", 80.0f, 130.0, DisplayName = "ModifyBooking: Cheaper seat")]
        [DataRow("7A", "Economy Plus", 120.0f, 170.0, DisplayName = "ModifyBooking: Mid-range seat")]
        [DataRow("15F", "Economy", 100.0f, 150.0, DisplayName = "ModifyBooking: Same price, different seat")]
        public void ModifyBooking_WithNewSeatOnly_UpdatesBookingCorrectly(
            string newSeatId, string newSeatClass, float newSeatPrice, double expectedTotalPrice)
        {
            // Arrange
            int bookingId = 1;
            int flightId = 1;
            string originalSeatId = "6F";
            int fixedLuggageAmount = 1;
            bool hasInsurance = false;

            var originalBooking = new BookingModel
            {
                BookingID = bookingId,
                FlightID = flightId,
                UserID = SessionManager.CurrentUser.UserID,
                SeatID = originalSeatId,
                LuggageAmount = fixedLuggageAmount,
                HasInsurance = hasInsurance,
                TotalPrice = 150.0m
            };

            var newSeat = new SeatModel { SeatID = newSeatId, SeatType = newSeatClass, Price = newSeatPrice };

            mockBookingAccess.Setup(b => b.GetBookingById(bookingId)).Returns(originalBooking);
            mockFlightAccess.Setup(f => f.GetById(flightId)).Returns(new FlightModel { FlightID = flightId });

            // Act
            bool result = BookingLogic.ModifyBooking(bookingId, newSeat, fixedLuggageAmount);

            // Assert
            Assert.IsTrue(result, "Booking modification should be successful.");

            mockFlightSeatAccess.Verify(fs => fs.SetSeatOccupancy(flightId, originalSeatId, false), Times.Once);
            mockFlightSeatAccess.Verify(fs => fs.SetSeatOccupancy(flightId, newSeat.SeatType, true), Times.Once);

            mockBookingAccess.Verify(b => b.UpdateBooking(It.Is<BookingModel>(bm =>
                bm.BookingID == bookingId &&
                bm.SeatID == newSeatId &&
                bm.SeatClass == newSeatClass &&
                bm.LuggageAmount == fixedLuggageAmount &&
                bm.TotalPrice == (decimal)expectedTotalPrice
            )), Times.Once, "UpdateBooking was not called with the correct new seat details and price.");
        }

        [TestMethod]
        public void GetBookingById_WhenBookingExists_ReturnsBooking()
        {
            // Arrange
            int bookingIdToRetrieve = 1;
            BookingModel expectedBooking = new BookingModel
            {
                BookingID = bookingIdToRetrieve,
                UserID = 1,
                FlightID = 100,
                SeatID = "1A",
                BookingStatus = "Confirmed"
            };

            // Setup the mock to return expected booking
            mockBookingAccess.Setup(service => service.GetBookingById(bookingIdToRetrieve))
                             .Returns(expectedBooking);

            // Act
            BookingModel actualBooking = BookingLogic.GetBookingById(bookingIdToRetrieve);

            // Assert
            mockBookingAccess.Verify(service => service.GetBookingById(bookingIdToRetrieve), Times.Once,
                "BookingAccessService.GetBookingById was not called or called an unexpected number of times.");

            Assert.IsNotNull(actualBooking, "Expected to find a booking, but null was returned.");
            Assert.AreEqual(expectedBooking.BookingID, actualBooking.BookingID, "The BookingID of the returned booking does not match the expected ID.");
            Assert.AreSame(expectedBooking, actualBooking, "The returned booking object is not the same instance as expected from the mock.");
        }

        [TestMethod]
        public void GetBookingById_WhenBookingDoesNotExist_ReturnsNull()
        {
            // Arrange
            int bookingIdToRetrieve = 9999;

            // Setup the mock to return null
            mockBookingAccess.Setup(service => service.GetBookingById(bookingIdToRetrieve))
                             .Returns((BookingModel)null);

            // Act
            BookingModel actualBooking = BookingLogic.GetBookingById(bookingIdToRetrieve);

            // Assert
            mockBookingAccess.Verify(service => service.GetBookingById(bookingIdToRetrieve), Times.Once,
                "BookingAccessService.GetBookingById was not called or called an unexpected number of times.");

            Assert.IsNull(actualBooking, "Expected null for a non-existent booking, but a booking was returned.");
        }
    }
}