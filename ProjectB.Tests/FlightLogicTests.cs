using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using ProjectB.DataAccess;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class FlightLogicTests
    {

        [DataTestMethod]
        [DataRow("A123", 150, "Test Airline", "JFK", "LAX", 200, true, DisplayName = "Valid flight, should succeed")]
        [DataRow("InvalidID", 0, "Test Airline", "JFK", "LAX", 200, false, DisplayName = "Invalid airplane, should fail")]
        [DataRow("A123", 150, "Test Airline", "JFK", "LAX", -100, false, DisplayName = "Invalid price, should fail")]
        public void AddFlight_WhenCalled_VerifiesWriteAndSeatCreation(
            string airplaneId,
            int totalSeats,
            string airline,
            string departureAirport,
            string arrivalAirport,
            int price,
            bool expectedResult)
        {
            // Arrange
            var mockFlightAccess = new Mock<IFlightAccess>();
            var mockAirplaneAccess = new Mock<IAirplaneAccess>();
            var mockFlightSeatAccess = new Mock<IFlightSeatAccess>();

            FlightLogic.FlightAccessService = mockFlightAccess.Object;
            FlightLogic.AirplaneAccessService = mockAirplaneAccess.Object;
            FlightLogic.FlightSeatAccessService = mockFlightSeatAccess.Object;

            mockFlightAccess.Setup(f => f.GetAllFlightData()).Returns(new List<FlightModel>());
            var airplane = totalSeats > 0 ? new AirplaneModel { AirplaneID = airplaneId, TotalSeats = totalSeats } : null;
            mockAirplaneAccess.Setup(a => a.GetAirplaneData(airplaneId)).Returns(airplane);

            var flight = new FlightModel
            {
                FlightID = 0,
                Airline = airline,
                AirplaneID = airplaneId,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                DepartureTime = DateTime.Now.AddDays(1),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(5),
                Price = price,
                FlightStatus = "Scheduled"
            };

            // Act
            var result = FlightLogic.AddFlight(flight);

            // Assert
            Assert.AreEqual(expectedResult, result);

            if (expectedResult)
            {
                mockFlightAccess.Verify(f => f.Write(It.IsAny<FlightModel>()), Times.Once, "Write should be called once for valid input.");
                mockFlightSeatAccess.Verify(f => f.CreateFlightSeats(It.IsAny<int>(), It.IsAny<string>()), Times.Once, "CreateFlightSeats should be called once for valid input.");
            }
            else
            {
                mockFlightAccess.Verify(f => f.Write(It.IsAny<FlightModel>()), Times.Never, "Write should not be called for invalid input.");
                mockFlightSeatAccess.Verify(f => f.CreateFlightSeats(It.IsAny<int>(), It.IsAny<string>()), Times.Never, "CreateFlightSeats should not be called for invalid input.");
            }
        }

        [DataTestMethod]
        [DataRow(1, true, DisplayName = "Existing flight returns flight")]
        [DataRow(999, false, DisplayName = "Nonexistent flight returns null")]
        public void GetFlightById_ReturnsExpectedResult(int flightId, bool exists)
        {
            // Arrange
            var mockFlightAccess = new Mock<IFlightAccess>();
            var expectedFlight = exists ? new FlightModel { FlightID = flightId } : null;
            mockFlightAccess.Setup(f => f.GetById(flightId)).Returns(expectedFlight);
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

            // Act
            var result = FlightLogic.GetFlightById(flightId);

            // Assert
            if (exists)
                Assert.IsNotNull(result);
            else
                Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow(1, true, DisplayName = "Valid flightId returns true")]
        [DataRow(0, false, DisplayName = "Invalid flightId returns false")]
        public void DeleteFlight_ReturnsExpectedResult(int flightId, bool expectedResult)
        {
            // Arrange
            var mockFlightAccess = new Mock<IFlightAccess>();
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

            // Act
            var result = FlightLogic.DeleteFlight(flightId);

            // Assert
            if (expectedResult)
                mockFlightAccess.Verify(f => f.Delete(flightId), Times.Once);
            else
                mockFlightAccess.Verify(f => f.Delete(It.IsAny<int>()), Times.Never);

            Assert.AreEqual(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow("JFK", "LAX", "2025-06-12", 2, DisplayName = "Two flights match filter")]
        [DataRow("JFK", "ORD", "2025-06-12", 0, DisplayName = "No flights match filter")]
        public void GetFilteredFlights_ReturnsExpectedCount(string origin, string destination, string date, int expectedCount)
        {
            // Arrange
            var mockFlightAccess = new Mock<IFlightAccess>();
            var flights = new List<FlightModel>
            {
                new FlightModel { FlightID = 1, DepartureAirport = "JFK", ArrivalAirport = "LAX", DepartureTime = DateTime.Parse("2025-06-12") },
                new FlightModel { FlightID = 2, DepartureAirport = "JFK", ArrivalAirport = "LAX", DepartureTime = DateTime.Parse("2025-06-12") }
            };
            mockFlightAccess.Setup(f => f.GetFilteredFlights(origin, destination, DateTime.Parse(date)))
                .Returns(flights.FindAll(f => f.DepartureAirport == origin && f.ArrivalAirport == destination && f.DepartureTime.Date == DateTime.Parse(date).Date));
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

            // Act
            var result = FlightLogic.GetFilteredFlights(origin, destination, DateTime.Parse(date));

            // Assert
            Assert.AreEqual(expectedCount, result.Count);
        }

        [DataTestMethod]
        [DataRow(1, "Updated Airline", 300, true, DisplayName = "Valid update returns true")]
        [DataRow(0, "Updated Airline", 300, false, DisplayName = "Invalid FlightID throws exception")]
        public void UpdateFlight_ReturnsExpectedResult(int flightId, string airline, int price, bool shouldSucceed)
        {
            // Arrange
            var mockFlightAccess = new Mock<IFlightAccess>();
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

            var flight = new FlightModel
            {
                FlightID = flightId,
                Airline = airline,
                AirplaneID = "A123",
                DepartureAirport = "JFK",
                ArrivalAirport = "LAX",
                DepartureTime = DateTime.Now.AddDays(1),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(5),
                Price = price,
                AvailableSeats = 100,
                FlightStatus = "Scheduled"
            };

            if (shouldSucceed)
            {
                mockFlightAccess.Setup(f => f.Update(It.IsAny<FlightModel>()));
                // Act
                var result = FlightLogic.UpdateFlight(flight);
                // Assert
                Assert.IsTrue(result);
                mockFlightAccess.Verify(f => f.Update(It.IsAny<FlightModel>()), Times.Once);
            }
            else
            {
                Assert.ThrowsException<ArgumentException>(() => FlightLogic.UpdateFlight(flight));
                mockFlightAccess.Verify(f => f.Update(It.IsAny<FlightModel>()), Times.Never);
            }
        }
    }
}