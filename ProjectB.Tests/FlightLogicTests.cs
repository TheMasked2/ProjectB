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
    public class FlightLogicTests
    {
        private Mock<IFlightAccess> mockFlightAccess;
        private Mock<IFlightSeatAccess> mockFlightSeatAccess;
        private Mock<IBookingAccess> mockBookingAccess;


        [TestInitialize]
        public void Setup()
        {
            mockFlightSeatAccess = new Mock<IFlightSeatAccess>();
            mockFlightAccess = new Mock<IFlightAccess>();
            mockBookingAccess = new Mock<IBookingAccess>();
            BookingLogic.BookingAccessService = mockBookingAccess.Object;
            FlightLogic.FlightAccessService = mockFlightAccess.Object;
            FlightLogic.FlightSeatAccessService = mockFlightSeatAccess.Object;
        }

        [DataTestMethod]
        [DataRow("ARK", "CAL", "2025-07-10", false, 1, DisplayName = "Arrakeen to Caladan, scheduled, 1 match")]
        [DataRow("GPR", "CAL", "2025-07-10", false, 1, DisplayName = "Giedi Prime to Caladan, scheduled, 1 match")]
        [DataRow("ARK", "SAL", "2025-07-10", false, 0, DisplayName = "Arrakeen to Salusa Secundus, no match")]
        [DataRow("ARK", "CAL", "2025-07-11", false, 0, DisplayName = "Arrakeen to Caladan, wrong date, no match")]
        [DataRow("ARK", "CAL", "2025-07-10", true, 0, DisplayName = "Arrakeen to Caladan, past flights, no match")]
        public void GetFilteredFlights_ReturnsExpectedFlights(
            string origin,
            string destination,
            string date,
            bool past,
            int expectedCount)
        {
            // Arrange
            List<FlightModel> mockFlights = new List<FlightModel>
            {
                new FlightModel
                {
                    FlightID = 1,
                    DepartureAirport = "ARK",
                    ArrivalAirport = "CAL",
                    DepartureTime = DateTime.Parse("2025-07-10"),
                    ArrivalTime = DateTime.Parse("2025-07-10 15:00"),
                    Airline = "Airtreides",
                    AirplaneID = "HEIGH",
                    Status = "Scheduled"
                },
                new FlightModel
                {
                    FlightID = 2,
                    DepartureAirport = "GPR",
                    ArrivalAirport = "CAL",
                    DepartureTime = DateTime.Parse("2025-07-10"),
                    ArrivalTime = DateTime.Parse("2025-07-10 18:00"),
                    Airline = "Airtreides",
                    AirplaneID = "FRIG1",
                    Status = "Scheduled"
                }
            };

            // Setup mock
            mockFlightAccess.Setup(x => x.GetFilteredFlights(
                origin, destination, DateTime.Parse(date), past)).Returns(
                mockFlights.Where(f =>
                    f.DepartureAirport == origin &&
                    f.ArrivalAirport == destination &&
                    f.DepartureTime.Date == DateTime.Parse(date).Date &&
                    !past && f.Status == "Scheduled"
                ).ToList()
            );

            // Act
            List<FlightModel> results = FlightLogic.GetFilteredFlights(
                origin, destination, DateTime.Parse(date), past);

            // Assert
            Assert.AreEqual(expectedCount, results.Count);
        }

        [DataTestMethod]
        [DataRow(1, true, DisplayName = "Flight with ID 1 exists")]
        [DataRow(2, true, DisplayName = "Flight with ID 2 exists")]
        [DataRow(999, false, DisplayName = "Flight with ID 999 does not exist")]
        public void GetFlightById_IfExists_ReturnsCorrectFlight(int flightId, bool flightExists)
        {
            // Arrange
            var mockFlights = new List<FlightModel>
            {
                new FlightModel
                {
                    FlightID = 1,
                    DepartureAirport = "ARK",
                    ArrivalAirport = "CAL",
                    DepartureTime = DateTime.Parse("2025-07-10"),
                    ArrivalTime = DateTime.Parse("2025-07-10 15:00"),
                    Airline = "Airtreides",
                    AirplaneID = "HEIGH",
                    Status = "Scheduled"
                },
                new FlightModel
                {
                    FlightID = 2,
                    DepartureAirport = "GPR",
                    ArrivalAirport = "CAL",
                    DepartureTime = DateTime.Parse("2025-07-10"),
                    ArrivalTime = DateTime.Parse("2025-07-10 18:00"),
                    Airline = "Airtreides",
                    AirplaneID = "FRIG1",
                    Status = "Scheduled"
                }
            };

            // Setup mock
            mockFlightAccess.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns((int id) => mockFlights.FirstOrDefault(f => f.FlightID == id));

            // Act
            var result = FlightLogic.FlightAccessService.GetById(flightId);

            // Assert
            if (flightExists)
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(flightId, result.FlightID);
            }
            else
            {
                Assert.IsNull(result);
            }
        }

        [DataTestMethod]
        [DataRow("ARK", "CAL", "2025-07-10 08:00", "2025-07-10 15:00", "Airtreides", "HEIGH", DisplayName = "Add Arrakeen to Caladan flight with Heighliner")]
        [DataRow("GPR", "CAL", "2025-07-11 09:00", "2025-07-11 16:00", "Airtreides", "FRIG1", DisplayName = "Add Giedi Prime to Caladan flight with Frigate")]
        [DataRow("LHR", "JFK", "2025-07-12 10:00", "2025-07-12 18:00", "Imperial Airways", "A380", DisplayName = "Add London to New York flight with A380")]
        public void AddFlight_SetsStatusAndCreatesSeatMap(
            string departureAirport,
            string arrivalAirport,
            string departureTime,
            string arrivalTime,
            string airline,
            string airplaneId)
        {
            // Arrange
            FlightLogic.FlightAccessService = mockFlightAccess.Object;
            SeatMapLogic.FlightSeatAccessService = mockFlightSeatAccess.Object;

            FlightModel flight = new()
            {
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                DepartureTime = DateTime.Parse(departureTime),
                ArrivalTime = DateTime.Parse(arrivalTime),
                Airline = airline,
                AirplaneID = airplaneId
            };

            // Setup mocks
            mockFlightAccess.Setup(x => x.Insert(It.IsAny<FlightModel>()));
            mockFlightAccess.Setup(x => x.GetFlightIdByDetails(It.IsAny<FlightModel>())).Returns(42);
            mockFlightSeatAccess.Setup(x => x.HasAnySeatsForFlight(42)).Returns(false);
            mockFlightSeatAccess.Setup(x => x.CreateFlightSeats(42, airplaneId));

            // Act
            FlightLogic.AddFlight(flight);

            // Assert
            Assert.AreEqual("Scheduled", flight.Status);
            mockFlightAccess.Verify(x => x.Insert(It.Is<FlightModel>(f => f == flight)), Times.Once);
            mockFlightAccess.Verify(x => x.GetFlightIdByDetails(It.Is<FlightModel>(f => f == flight)), Times.Once);
            mockFlightSeatAccess.Verify(x => x.HasAnySeatsForFlight(42), Times.Once);
            mockFlightSeatAccess.Verify(x => x.CreateFlightSeats(42, airplaneId), Times.Once);
            Assert.AreEqual(42, flight.FlightID);
        }

        [DataTestMethod]
        [DataRow(1, "Scheduled", "Delayed", DisplayName = "Update status from Scheduled to Delayed")]
        [DataRow(2, "Scheduled", "Cancelled", DisplayName = "Update status from Scheduled to Cancelled")]
        [DataRow(3, "Scheduled", "Boarding", DisplayName = "Update status from Scheduled to Boarding")]
        public void UpdateFlight_UpdatesFlightInDataStore(int flightId, string initialStatus, string newStatus)
        {
            // Arrange
            List<FlightModel> updatedFlights = [];
            FlightModel flight = new()
            {
                FlightID = flightId,
                DepartureAirport = "ARK",
                ArrivalAirport = "CAL",
                DepartureTime = DateTime.Parse("2025-07-10"),
                ArrivalTime = DateTime.Parse("2025-07-10 15:00"),
                Airline = "Airtreides",
                AirplaneID = "HEIGH",
                Status = initialStatus
            };

            // Setup mock
            mockFlightAccess.Setup(x => x.Update(It.IsAny<FlightModel>()))
                .Callback<FlightModel>(f => updatedFlights.Add(f));

            // Act
            flight.Status = newStatus;
            FlightLogic.UpdateFlight(flight);

            // Assert
            Assert.AreEqual(1, updatedFlights.Count);
            Assert.AreEqual(flightId, updatedFlights[0].FlightID);
            Assert.AreEqual(newStatus, updatedFlights[0].Status);
        }
    
        [DataTestMethod]
        [DataRow(1, DisplayName = "Delete flight with ID 1")]
        [DataRow(2, DisplayName = "Delete flight with ID 2")]
        [DataRow(999, DisplayName = "Delete flight with ID 999 (nonexistent)")]
        public void DeleteFlight_DeletesSeatsBookingsAndFlight(int flightId)
        {
            // Arrange
            // Setup mock
            mockBookingAccess.Setup(x => x.GetBookingsByFlightId(flightId)).Returns(new List<BookingModel>
            {
                new BookingModel { BookingID = 10, FlightID = flightId },
                new BookingModel { BookingID = 20, FlightID = flightId }
            });

            // Act
            FlightLogic.DeleteFlight(flightId);

            // Assert
            mockFlightSeatAccess.Verify(x => x.DeleteFlightSeatsByFlightID(flightId), Times.Once);
            mockBookingAccess.Verify(x => x.GetBookingsByFlightId(flightId), Times.Once);
            mockBookingAccess.Verify(x => x.Delete(10), Times.Once);
            mockBookingAccess.Verify(x => x.Delete(20), Times.Once);
            mockFlightAccess.Verify(x => x.Delete(flightId), Times.Once);
        }
    }        
}
