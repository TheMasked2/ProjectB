using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using ProjectB.DataAccess;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class PastFlightLogicTests
    {
        [TestMethod]
        public void GetFilteredPastFlights_ReturnsExpectedFlights()
        {
            // Arrange
            var mockPastFlightAccess = new Mock<IPastFlightAccess>();
            var expectedFlights = new List<FlightModel>
            {
                new FlightModel { FlightID = 1, DepartureAirport = "JFK", ArrivalAirport = "LAX", DepartureTime = DateTime.Parse("2024-06-01") },
                new FlightModel { FlightID = 2, DepartureAirport = "JFK", ArrivalAirport = "LAX", DepartureTime = DateTime.Parse("2024-06-02") }
            };

            mockPastFlightAccess.Setup(x => x.GetFilteredPastFlights("JFK", "LAX", DateTime.Parse("2024-06-01")))
                .Returns(expectedFlights);

            PastFlightLogic.PastFlightAccessService = mockPastFlightAccess.Object;

            // Act
            var result = PastFlightLogic.GetFilteredPastFlights("JFK", "LAX", DateTime.Parse("2024-06-01"));

            // Assert
            Assert.AreEqual(expectedFlights.Count, result.Count);
            Assert.AreEqual(expectedFlights[0].FlightID, result[0].FlightID);
            Assert.AreEqual(expectedFlights[1].FlightID, result[1].FlightID);
        }
        [TestMethod]
        public void GetFilteredPastFlights_ReturnsEmptyList_WhenNoFlightsMatch()
        {
            // Arrange
            var mockPastFlightAccess = new Mock<IPastFlightAccess>();
            mockPastFlightAccess.Setup(x => x.GetFilteredPastFlights("ABC", "XYZ", DateTime.Parse("2024-01-01")))
                .Returns(new List<FlightModel>());

            PastFlightLogic.PastFlightAccessService = mockPastFlightAccess.Object;

            // Act
            var result = PastFlightLogic.GetFilteredPastFlights("ABC", "XYZ", DateTime.Parse("2024-01-01"));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}