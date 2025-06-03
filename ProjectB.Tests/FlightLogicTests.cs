using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectB.DataAccess;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class FlightLogicTests
    {
        [DataTestMethod]
        [DataRow("A123", 150, "Test Airline", "JFK", "LAX", 200, true)] // Valid flight
        [DataRow("InvalidID", 0, "Test Airline", "JFK", "LAX", 200, false)] // Invalid airplane
        [DataRow("A123", 150, "Test Airline", "JFK", "LAX", -100, false)] // Invalid price
        public void AddFlight_CallsWriteAndSeatCreation_ReturnsExpected(
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

            var flight = new FlightModel
            {
                FlightID = 0, // Will be auto-incremented
                Airline = airline,
                AirplaneID = airplaneId,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                DepartureTime = DateTime.Now.AddDays(1),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(5),
                Price = price,
                FlightStatus = "Scheduled"
            };

            mockAirplaneAccess.Setup(a => a.GetAirplaneData(airplaneId)).Returns(airplane);

            // Act
            var result = FlightLogic.AddFlight(flight);

            if (expectedResult)
            {
                Assert.IsTrue(result);
                mockFlightAccess.Verify(f => f.Write(It.IsAny<FlightModel>()), Times.Once);
                mockFlightSeatAccess.Verify(f => f.CreateFlightSeats(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            }
            else
            {
                Assert.IsFalse(result);
                mockFlightAccess.Verify(f => f.Write(It.IsAny<FlightModel>()), Times.Never);
                mockFlightSeatAccess.Verify(f => f.CreateFlightSeats(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}