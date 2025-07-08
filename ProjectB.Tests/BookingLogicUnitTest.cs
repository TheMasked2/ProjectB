using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Role = UserRole.Customer,
                FirstTimeDiscount = false,
                BirthDate = DateTime.Now.AddYears(-30) // A standard non-senior user
            };
            SessionManager.SetCurrentUser(user);
        }


        [DataTestMethod]
        // Roles and Base Prices
        [DataRow(false, UserRole.Guest, -30, 0, false, null, 100.0, 1.0, 0.0, DisplayName = "Base Price: Guest")]
        [DataRow(false, UserRole.Customer, -30, 0, false, null, 100.0, 1.0, 0.0, DisplayName = "Base Price: Customer")]
        // Options (Luggage & Insurance)
        [DataRow(false, UserRole.Customer, -30, 0, true, null, 120.0, 1.0, 20.0, DisplayName = "Option: Insurance")]
        [DataRow(false, UserRole.Customer, -30, 2, false, null, 200.0, 1.0, 0.0, DisplayName = "Option: 2 Luggage")]
        [DataRow(false, UserRole.Customer, -30, 1, true, null, 170.0, 1.0, 20.0, DisplayName = "Option: Luggage + Insurance")]
        // Single Discounts (Customer Only)
        [DataRow(true, UserRole.Customer, -30, 0, false, null, 90.0, 0.9, 0.0, DisplayName = "Discount: First-Time")]
        [DataRow(false, UserRole.Customer, -70, 0, false, null, 80.0, 0.8, 0.0, DisplayName = "Discount: Senior")]
        [DataRow(false, UserRole.Customer, -30, 0, false, Coupons.Atreides, 85.0, 0.85, 0.0, DisplayName = "Discount: Coupon (15%)")]
        // Multiple Discounts
        [DataRow(true, UserRole.Customer, -70, 0, false, null, 70.0, 0.7, 0.0, DisplayName = "Discount: First-Time + Senior")]
        [DataRow(true, UserRole.Customer, -30, 0, false, Coupons.MuadDib, 70.0, 0.7, 0.0, DisplayName = "Discount: First-Time + Coupon (20%)")]
        [DataRow(false, UserRole.Customer, -70, 0, false, Coupons.BeneGesserit, 70.0, 0.7, 0.0, DisplayName = "Discount: Senior + Coupon (10%)")]
        [DataRow(true, UserRole.Customer, -70, 0, false, Coupons.LisanAlGaib, 60.0, 0.6, 0.0, DisplayName = "Discount: All Three (10+20+10)")]
        // Edge Case: 100% Discount
        [DataRow(true, UserRole.Customer, -70, 0, false, Coupons.KwisatzHaderach, 0.0, 0.0, 0.0, DisplayName = "Edge Case: 100% Discount (10+20+70)")]
        [DataRow(true, UserRole.Customer, -70, 2, true, Coupons.KwisatzHaderach, 0.0, 0.0, 20.0, DisplayName = "Edge Case: 100% Discount with extras")]
        // Guest discount checks (should not receive age/first-time discounts)
        [DataRow(true, UserRole.Guest, -30, 0, false, null, 100.0, 1.0, 0.0, DisplayName = "Guest Check: No First-Time Discount")]
        [DataRow(false, UserRole.Guest, -70, 0, false, null, 100.0, 1.0, 0.0, DisplayName = "Guest Check: No Senior Discount")]
        [DataRow(true, UserRole.Guest, -70, 0, false, Coupons.Atreides, 85.0, 0.85, 0.0, DisplayName = "Guest Check: Coupon Only")]
        // Spice Coupon Easter Egg
        [DataRow(false, UserRole.Customer, -30, 0, false, Coupons.Spice, -100.0, 1.0, 0.0, DisplayName = "Spice Coupon: Base")]
        [DataRow(true, UserRole.Customer, -70, 2, true, Coupons.Spice, -154.0, 0.7, 20.0, DisplayName = "Spice Coupon: With other discounts and extras")]
        public void CalculateBookingPrice_ReturnsCorrectAmounts(
            bool firstTimeDiscount, UserRole role, int ageYears,
            int luggage, bool insurance, Coupons? coupon,
            double expectedPrice, double expectedDiscount, double expectedInsurance)
        {
            // Arrange
            User user = new User
            {
                FirstTimeDiscount = firstTimeDiscount,
                Role = role,
                BirthDate = DateTime.Now.AddYears(ageYears)
            };
            FlightModel flight = new FlightModel { FlightID = 1 };
            SeatModel seat = new SeatModel { SeatID = "1A", Price = 100.0m, SeatClass = "Economy" };

            // Act
            var result = BookingLogic.CalculateBookingPrice(user, flight, seat, luggage, insurance, coupon);

            // Assert
            Assert.AreEqual((decimal)expectedPrice, result.finalPrice, 0.01m, "Final price calculation is incorrect");
            Assert.AreEqual((decimal)expectedDiscount, result.discount, 0.01m, "Discount calculation is incorrect");
            Assert.AreEqual((decimal)expectedInsurance, result.insurancePrice, 0.01m, "Insurance price calculation is incorrect");
        }

        [DataTestMethod]
        [DataRow(123, 456, "B737-15A", "Pending", 1, true, null, 290.0, 1.0, DisplayName = "Regular booking with luggage and insurance")]
        [DataRow(789, 101, "B737-7B", "Pending", 0, false, Coupons.Spice, -200.0, 1.0, DisplayName = "Spice coupon booking (negative price easter egg)")]
        [DataRow(456, 789, "B737-5C", "Pending", 2, true, Coupons.BeneGesserit, 560.0, 0.8, DisplayName = "Booking with discounts, luggage, and insurance")]
        public void BookingBuilder_CreatesCorrectBookingModels(
            int userId, int flightId, string seatId,
            string expectedStatus,
            int luggageAmount, bool hasInsurance, Coupons? coupon,
            double expectedPrice, double expectedDiscount)
        {
            // Arrange
            User user = new() { UserID = userId, Role = UserRole.Customer, BirthDate = DateTime.Now.AddYears(-30), FirstTimeDiscount = (userId == 456) };
            FlightModel flight = new() { FlightID = flightId };
            SeatModel seat = new() { SeatID = seatId, Price = (userId == 456) ? 500.0m : 200.0m };

            // Act
            BookingModel result = BookingLogic.BookingBuilder(user, flight, seat, coupon, luggageAmount, hasInsurance);

            // Assert
            Assert.IsNotNull(result, "The resulting booking model should not be null.");
            Assert.AreEqual(expectedStatus, result.BookingStatus, "The booking status was not set correctly.");
            Assert.AreEqual(userId, result.UserID, "The UserID was not set correctly.");
            Assert.AreEqual(flightId, result.FlightID, "The FlightID was not set correctly.");
            Assert.AreEqual(seatId, result.SeatID, "The SeatID was not set correctly.");
            Assert.AreEqual(luggageAmount, result.LuggageAmount, "The luggage amount was not set correctly.");
            Assert.AreEqual(hasInsurance, result.HasInsurance, "The insurance status was not set correctly.");
            Assert.AreEqual((decimal)expectedDiscount, result.Discount, 0.01m, "The calculated discount is incorrect.");
            Assert.AreEqual((decimal)expectedPrice, result.TotalPrice, 0.01m, "The calculated total price is incorrect.");
        }

        [DataTestMethod]
        [DataRow(1, true, new[] { 1, 3 }, DisplayName = "User 1: Upcoming bookings")]
        [DataRow(1, false, new[] { 2, 5, 8 }, DisplayName = "User 1: Past bookings")]
        [DataRow(2, true, new[] { 4 }, DisplayName = "User 2: Upcoming booking")]
        [DataRow(2, false, new[] { 6 }, DisplayName = "User 2: Past booking")]
        [DataRow(3, true, new int[] { }, DisplayName = "User 3: No upcoming bookings")]
        [DataRow(3, false, new[] { 7 }, DisplayName = "User 3: Past booking")]
        public void GetBookingsForUser_ReturnsCorrectBookings(int userId, bool upcoming, int[] expectedBookingIds)
        {
            // Arrange
            DateTime now = DateTime.Now;
            DateTime yesterday = now.AddDays(-1);
            DateTime tomorrow = now.AddDays(1);

            // Create mock data for flights
            var flights = new Dictionary<int, FlightModel>
            {
                { 1, new FlightModel { FlightID = 1, DepartureTime = tomorrow } },
                { 2, new FlightModel { FlightID = 2, DepartureTime = yesterday } },
                { 3, new FlightModel { FlightID = 3, DepartureTime = tomorrow.AddDays(3) } },
                { 4, new FlightModel { FlightID = 4, DepartureTime = tomorrow.AddDays(5) } },
                { 5, new FlightModel { FlightID = 5, DepartureTime = yesterday.AddDays(-2) } },
                { 6, new FlightModel { FlightID = 6, DepartureTime = yesterday.AddDays(-5) } },
                { 7, new FlightModel { FlightID = 7, DepartureTime = yesterday.AddDays(-1) } },
                { 8, new FlightModel { FlightID = 8, DepartureTime = yesterday } }
            };

            // Create bookings for multiple users
            List<BookingModel> allBookings = new List<BookingModel>
            {
                new BookingModel { BookingID = 1, UserID = 1, FlightID = 1, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 2, UserID = 1, FlightID = 2, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 3, UserID = 1, FlightID = 3, BookingStatus = "Pending" },
                new BookingModel { BookingID = 4, UserID = 2, FlightID = 4, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 5, UserID = 1, FlightID = 5, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 6, UserID = 2, FlightID = 6, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 7, UserID = 3, FlightID = 7, BookingStatus = "Confirmed" },
                new BookingModel { BookingID = 8, UserID = 1, FlightID = 8, BookingStatus = "Cancelled" }
            };

            // Setup mocks
            mockBookingAccess
                .Setup(mock => mock.GetBookingsByUser(userId))
                .Returns(allBookings.Where(b => b.UserID == userId).ToList());

            mockFlightAccess
                .Setup(mock => mock.GetById(It.IsAny<int>()))
                .Returns<int>(id => flights.ContainsKey(id) ? flights[id] : null);

            // Act
            List<BookingModel> result = BookingLogic.GetBookingsForUser(userId, upcoming);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(expectedBookingIds.Length, result.Count, $"Expected {expectedBookingIds.Length} bookings but got {result.Count}.");

            var resultIds = result.Select(b => b.BookingID).ToList();
            CollectionAssert.AreEquivalent(expectedBookingIds, resultIds, "The list of returned booking IDs does not match the expected list.");
        }

        [DataTestMethod]
        [DataRow(123, "Pending", 1, 101, "B737-1A", 0, false, 1.0, 200.0, DisplayName = "Confirm a standard pending booking")]
        [DataRow(456, "Pending", 2, 202, "B737-2B", 1, true, 1.0, 350.0, DisplayName = "Confirm a booking with luggage and insurance")]
        [DataRow(789, "Pending", 3, 303, "B737-3C", 2, false, 0.85, 500.0, DisplayName = "Confirm a booking with 2 luggage and a coupon discount")]
        public void ConfirmBooking_SetsStatusAndInsertsBooking(
            int bookingId, string initialStatus, int userId, int flightId, string seatId,
            int luggageAmount, bool hasInsurance, double discount, double totalPrice)
        {
            // Arrange
            var initialBooking = new BookingModel
            {
                BookingID = bookingId,
                BookingStatus = initialStatus,
                UserID = userId,
                FlightID = flightId,
                SeatID = seatId,
                LuggageAmount = luggageAmount,
                HasInsurance = hasInsurance,
                Discount = (decimal)discount,
                TotalPrice = (decimal)totalPrice
            };

            // Setup mocks
            List<BookingModel> mockBookingDb = [];
            mockBookingAccess
                .Setup(service => service.Insert(It.IsAny<BookingModel>()))
                .Callback<BookingModel>(b => mockBookingDb.Add(b));

            // Act
            BookingLogic.BookTheDamnFlight(initialBooking);

            // Assert
            BookingModel insertedBooking = mockBookingDb.First();
            Assert.AreEqual(1, mockBookingDb.Count, "A single booking should have been inserted.");
            CollectionAssert.Contains(mockBookingDb, initialBooking, "The confirmed booking was not found in the list of inserted bookings.");
            Assert.AreEqual("Confirmed", insertedBooking.BookingStatus, "The booking status should be set to 'Confirmed'.");
            Assert.AreEqual(initialBooking.BookingID, insertedBooking.BookingID, "The BookingID should not be changed.");
            Assert.AreEqual(initialBooking.UserID, insertedBooking.UserID, "The UserID should not be changed.");
            Assert.AreEqual(initialBooking.TotalPrice, insertedBooking.TotalPrice, "The TotalPrice should not be changed.");
        }

        [DataTestMethod]
        [DataRow(1, false, false, 0.0, false, false, 0.0, DisplayName = "CancelBooking_WhenBookingNotFound")]
        [DataRow(2, true, true, 250.0, true, true, 0.0, DisplayName = "CancelBooking_WithInsurance_ReturnsFullRefund")]
        [DataRow(3, true, false, 250.0, true, false, 150.0, DisplayName = "CancelBooking_NoInsurance_AppliesFee")]
        [DataRow(4, true, false, 50.0, true, false, 0.0, DisplayName = "CancelBooking_NoInsurance_PriceBecomesZero")]
        public void CancelBooking_HandlesScenariosAndUpdatesState(
            int bookingId,
            bool bookingExists,
            bool hasInsurance,
            double initialPrice,
            bool expectedSuccess,
            bool expectedFreeCancel,
            double expectedFinalPrice)
        {
            // Arrange
            BookingModel mockBooking = null;
            if (bookingExists)
            {
                mockBooking = new BookingModel
                {
                    BookingID = bookingId,
                    FlightID = 101,
                    SeatID = "1A",
                    HasInsurance = hasInsurance,
                    TotalPrice = (decimal)initialPrice,
                    BookingStatus = "Confirmed"
                };
            }

            // Setup mock
            mockBookingAccess.Setup(s => s.GetById(bookingId)).Returns(mockBooking);

            // Act
            (bool success, bool freeCancel) result = BookingLogic.CancelBooking(bookingId);

            // Assert
            Assert.AreEqual(expectedSuccess, result.success, "Success status was not as expected.");
            Assert.AreEqual(expectedFreeCancel, result.freeCancel, "Free cancel status was not as expected.");

            if (expectedSuccess)
            {
                mockFlightSeatAccess.Verify(s => s.SetSeatOccupancy(mockBooking.FlightID, mockBooking.SeatID, false), Times.Once, "Update should have been called to free the seat.");
                mockBookingAccess.Verify(s => s.Update(It.Is<BookingModel>(b =>
                    b.BookingID == bookingId &&
                    b.BookingStatus == "Cancelled" &&
                    b.TotalPrice == (decimal)expectedFinalPrice
                )), Times.Once, "Update was not called with the correct final booking state.");
            }
            else
            {
                mockFlightSeatAccess.Verify(s => s.SetSeatOccupancy(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never, "SetSeatOccupancy should not have been called for a failed cancellation.");
                mockBookingAccess.Verify(s => s.Update(It.IsAny<BookingModel>()), Times.Never, "Update should not have been called for a failed cancellation.");
            }
        }


        [DataTestMethod]
        // The expected total price is the new seat price + a fixed luggage cost of $50.
        [DataRow("B737-1A", "Luxury", 400.0, 450.0, DisplayName = "ModifyBooking: To Luxury")]
        [DataRow("B737-2B", "Business", 250.0, 300.0, DisplayName = "ModifyBooking: To Business")]
        [DataRow("B737-7A", "Premium", 150.0, 200.0, DisplayName = "ModifyBooking: To Premium")]
        [DataRow("B737-10D", "Extra Legroom", 120.0, 170.0, DisplayName = "ModifyBooking: To Extra Legroom")]
        [DataRow("B737-15F", "Economy", 100.0, 150.0, DisplayName = "ModifyBooking: To Economy")]
        public void ModifyBooking_WhenSuccessful_UpdatesBookingWithNewSeatAndPrice(
             string newSeatId, string newSeatClass, double newSeatPrice, double expectedTotalPrice)
        {
            // Arrange
            const int bookingId = 1;
            const int flightId = 101;
            const string originalSeatId = "B737-20C";
            const int luggageAmount = 1; // Fixed for this test

            BookingModel originalBooking = new()
            {
                BookingID = bookingId,
                FlightID = flightId,
                SeatID = originalSeatId,
                LuggageAmount = luggageAmount,
                HasInsurance = false,
                TotalPrice = 150.0m // Initial price doesn't matter as it gets recalculated
            };

            SeatModel newSeat = new() { SeatID = newSeatId, SeatClass = newSeatClass, Price = (decimal)newSeatPrice };
            FlightModel flight = new() { FlightID = flightId };
            User user = new() { UserID = 1, Role = UserRole.Customer, BirthDate = DateTime.Now.AddYears(-30) };
            SessionManager.SetCurrentUser(user);

            // Setup mocks
            mockBookingAccess.Setup(s => s.GetById(bookingId)).Returns(originalBooking);
            mockFlightAccess.Setup(s => s.GetById(flightId)).Returns(flight);

            // Act
            bool result = BookingLogic.ModifyBooking(bookingId, newSeat, luggageAmount);

            // Assert
            Assert.IsTrue(result, "Booking modification should return true for a valid booking.");

            // Verify seat changes
            mockFlightSeatAccess.Verify(s => s.SetSeatOccupancy(flightId, originalSeatId, false), Times.Once, "The original seat should be freed.");
            mockFlightSeatAccess.Verify(s => s.SetSeatOccupancy(flightId, newSeatId, true), Times.Once, "The new seat should be occupied.");

            // Verify the booking with the correct new details
            mockBookingAccess.Verify(s => s.Update(It.Is<BookingModel>(b =>
                b.BookingID == bookingId &&
                b.SeatID == newSeatId &&
                b.LuggageAmount == luggageAmount &&
                b.TotalPrice == (decimal)expectedTotalPrice
            )), Times.Once, "The booking was not updated with the correct details.");
        }

        [DataTestMethod]
        [DataRow(1, true, 11, 101, DisplayName = "GetBookingById: Should find booking 1")]
        [DataRow(2, true, 13, 103, DisplayName = "GetBookingById: Should find booking 2")]
        [DataRow(3, false, 0, 0, DisplayName = "GetBookingById: Should return null for non-existent booking")]
        public void GetBookingById_ReturnsCorrectBookingOrNull(int bookingIdToFetch, bool shouldExist, int expectedUserId, int expectedFlightId)
        {
            // Arrange
            List<BookingModel> mockBookingDb =
            [
                new() { BookingID = 1, UserID = 11, FlightID = 101 },
                new() { BookingID = 2, UserID = 13, FlightID = 103 }
            ];

            // Setup mock
            mockBookingAccess
                .Setup(service => service.GetById(It.IsAny<int>()))
                .Returns<int>(id => mockBookingDb.FirstOrDefault(b => b.BookingID == id));

            // Act
            BookingModel? result = BookingLogic.GetBookingById(bookingIdToFetch);

            // Assert
            mockBookingAccess.Verify(service => service.GetById(bookingIdToFetch), Times.Once);

            if (shouldExist)
            {
                Assert.IsNotNull(result, "Expected to find a booking, but null was returned.");
                Assert.AreEqual(bookingIdToFetch, result.BookingID, "The wrong booking was returned.");
                Assert.AreEqual(expectedUserId, result.UserID, "UserID does not match expected.");
                Assert.AreEqual(expectedFlightId, result.FlightID, "FlightID does not match expected.");
            }
            else
            {
                Assert.IsNull(result, "Expected null for a non-existent booking, but an object was returned.");
            }
        }

        [DataTestMethod]
        [DataRow(1, "B737-1A", 101, DisplayName = "BookTheDamnFlight: Should book flight with seat B737-1A")]
        [DataRow(2, "B737-2B", 102, DisplayName = "BookTheDamnFlight: Should book flight with seat B737-2B")]
        public void BookTheDamnFlight_BooksFlightWithConfirmedStatus(int userId, string seatId, int flightId)
        {
            // Arrange
            var insertedBookings = new List<BookingModel>();

            User user = new User { UserID = userId, Role = UserRole.Customer, BirthDate = DateTime.Now.AddYears(-30) };
            SessionManager.SetCurrentUser(user);

            FlightModel flight = new FlightModel { FlightID = flightId };
            SeatModel seat = new SeatModel { SeatID = seatId, Price = 100.0m, SeatClass = "Economy" };

            BookingModel booking = BookingLogic.BookingBuilder(user, flight, seat, null, 0, false);

            // Setup mocks
            mockFlightSeatAccess.Setup(s => s.SetSeatOccupancy(flightId, seatId, true));
            mockBookingAccess.Setup(s => s.Insert(It.IsAny<BookingModel>()))
                .Callback<BookingModel>(b => insertedBookings.Add(b));

            // Act
            BookingLogic.BookTheDamnFlight(booking);

            // Assert
            Assert.AreEqual(1, insertedBookings.Count, "Exactly one booking should have been inserted.");
            Assert.AreSame(booking, insertedBookings[0], "The inserted booking should be the same object as the original.");
            Assert.AreEqual("Confirmed", insertedBookings[0].BookingStatus, "The booking status should be set to 'Confirmed'.");
        }

        [DataTestMethod]
        [DataRow(42, 4, DisplayName = "DeleteBookingsByFlightId: Removes all bookings for flight 42")]
        [DataRow(99, 3, DisplayName = "DeleteBookingsByFlightId: Removes all bookings for flight 99")]
        public void DeleteBookingsByFlightId_RemovesBookingsForGivenFlight(int flightIdToDelete, int expectedRemaining)
        {
            // Arrange
            var allBookings = new List<BookingModel>
            {
                new BookingModel { BookingID = 1, FlightID = 42 },
                new BookingModel { BookingID = 2, FlightID = 42 },
                new BookingModel { BookingID = 3, FlightID = 99 },
                new BookingModel { BookingID = 4, FlightID = 99 },
                new BookingModel { BookingID = 5, FlightID = 100 },
                new BookingModel { BookingID = 6, FlightID = 99 }
            };

            // Setup mock
            mockBookingAccess
                .Setup(m => m.GetBookingsByFlightId(flightIdToDelete))
                .Returns(() => allBookings.Where(b => b.FlightID == flightIdToDelete).ToList());
            mockBookingAccess
                .Setup(m => m.Delete(It.IsAny<int>()))
                .Callback<int>(id =>
                {
                    BookingModel? toRemove = allBookings.FirstOrDefault(b => b.BookingID == id);
                    if (toRemove != null)
                        allBookings.Remove(toRemove);
                });

            // Act
            BookingLogic.DeleteBookingsByFlightId(flightIdToDelete);

            // Assert
            Assert.AreEqual(expectedRemaining, allBookings.Count, "Unexpected number of bookings remain after deletion.");
            Assert.IsFalse(allBookings.Any(b => b.FlightID == flightIdToDelete), "No bookings for the deleted flight should remain.");
        }
    }
}