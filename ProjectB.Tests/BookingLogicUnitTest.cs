using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
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

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            mockBookingAccess = new Mock<IBookingAccess>();
            mockFlightAccess = new Mock<IFlightAccess>();
            mockFlightSeatAccess = new Mock<IFlightSeatAccess>();

            BookingLogic.BookingAccessService = mockBookingAccess.Object;
            BookingLogic.FlightAccessService = mockFlightAccess.Object;
            BookingLogic.FlightSeatAccessService = mockFlightSeatAccess.Object;
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
        [DataRow(1, true, new[] { 1, 3 })]    // User 1, Upcoming bookings (IDs 1, 3)
        [DataRow(1, false, new[] { 2, 5, 8 })]// User 1, Past bookings (IDs 2, 5, 8)
        [DataRow(2, true, new[] { 4 })]       // User 2, Upcoming bookings (ID 4)
        [DataRow(2, false, new[] { 6 })]      // User 2, Past bookings (ID 6)
        [DataRow(3, true, new int[] { })]     // User 3, No upcoming bookings
        [DataRow(3, false, new[] { 7 })]      // User 3, Only past bookings (ID 7)
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
                new BookingModel { BookingID = 8, UserID = 1, DepartureTime = yesterday, BookingStatus = "Cancelled" }  // Cancelled bookings only show in past
            };

            // Setup mock
            mockBookingAccess
                .Setup(mock => mock.GetBookingsByUser(It.IsAny<int>()))
                .Returns<int>((userId) =>
                {
                    return allBookings
                        .Where(booking => booking.UserID == userId)
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

        // [DataTestMethod]
        // public void 
    }
}
