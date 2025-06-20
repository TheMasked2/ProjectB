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
        [DataRow(1, true)]
        [DataRow(999, false)]
        public void GetFlightById_Simple(int flightId, bool exists)
        {
            var mockFlightAccess = new Mock<IFlightAccess>();
            mockFlightAccess.Setup(f => f.GetById(flightId)).Returns(exists ? new FlightModel { FlightID = flightId } : null);
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

            var result = FlightLogic.GetFlightById(flightId);
            if (exists)
                Assert.IsNotNull(result);
            else
                Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow("JFK", "LAX", "2025-06-12", 2)]
        [DataRow("JFK", "ORD", "2025-06-12", 0)]
        public void GetFilteredFlights_Simple(string origin, string destination, string date, int expectedCount)
        {
            var mockFlightAccess = new Mock<IFlightAccess>();
            var flights = new List<FlightModel>
            {
                new FlightModel { FlightID = 1, DepartureAirport = "JFK", ArrivalAirport = "LAX", DepartureTime = DateTime.Parse("2025-06-12") },
                new FlightModel { FlightID = 2, DepartureAirport = "JFK", ArrivalAirport = "LAX", DepartureTime = DateTime.Parse("2025-06-12") }
            };
            mockFlightAccess.Setup(f => f.GetFilteredFlights(origin, destination, DateTime.Parse(date)))
                .Returns(flights.FindAll(f => f.DepartureAirport == origin && f.ArrivalAirport == destination && f.DepartureTime.Date == DateTime.Parse(date).Date));
            FlightLogic.FlightAccessService = mockFlightAccess.Object;

            var result = FlightLogic.GetFilteredFlights(origin, destination, DateTime.Parse(date));
            Assert.AreEqual(expectedCount, result.Count);
        }
    }
}