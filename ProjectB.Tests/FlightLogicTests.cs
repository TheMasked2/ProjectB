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
        public void AddFlight_TestCases(
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
            var mockAirplaneLogic = new Mock<IAirplaneLogic>();
            var mockFlightSeatAccess = new Mock<IFlightSeatAccess>();

            var flightLogic = new FlightLogic(mockFlightAccess.Object, mockAirplaneLogic.Object, mockFlightSeatAccess.Object);

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

            mockAirplaneLogic.Setup(a => a.GetAllAirplanes(airplaneId)).Returns(airplane);

            // Act
            if (expectedResult)
            {
                var result = flightLogic.AddFlight(flight);

                // Assert
                Assert.IsTrue(result);
                mockFlightAccess.Verify(f => f.Write(It.IsAny<FlightModel>()), Times.Once);
                mockFlightSeatAccess.Verify(f => f.CreateFlightSeats(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            }
            else
            {
                Assert.ThrowsException<ArgumentException>(() => flightLogic.AddFlight(flight));
                mockFlightAccess.Verify(f => f.Write(It.IsAny<FlightModel>()), Times.Never);
                mockFlightSeatAccess.Verify(f => f.CreateFlightSeats(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}