using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

namespace ProjectB.DataAccess
{
    public class PastFlightAccess : IPastFlightAccess
    {
        private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
        private const string Table = "PASTFLIGHTS";

        public void DeletePastFlights(DateTime monthAgo)
        {
            string sql = $@"DELETE FROM {Table}
                            WHERE DepartureTime < @MonthAgoDate";
            var parameters = new { MonthAgoDate = monthAgo };

            _connection.Execute(sql, parameters);
        }

        public void WritePastFlight(FlightModel flight)
        {
            string sql = $@"INSERT INTO {Table} 
                            (FlightID, Airline, AirplaneID, AvailableSeats, DepartureAirport, ArrivalAirport, 
                            DepartureTime, ArrivalTime, FlightStatus) 
                            VALUES 
                            (@flightID, @airline, @airplaneID, @availableSeats, 
                            @departureAirport, @arrivalAirport, @departureTime, @arrivalTime, @flightStatus)";
            _connection.Execute(sql, new
            {
                flightID = flight.FlightID,
                airline = flight.Airline,
                airplaneID = flight.AirplaneID,
                availableSeats = flight.AvailableSeats,
                departureAirport = flight.DepartureAirport,
                arrivalAirport = flight.ArrivalAirport,
                departureTime = flight.DepartureTime,
                arrivalTime = flight.ArrivalTime,
                flightStatus = flight.FlightStatus
            });
        }
        
        public List<FlightModel> GetPastFlights(DateTime currentDate)
        {
            string sql = $@"SELECT * FROM {Table}
                            WHERE DepartureTime < @CurrentDate";
            var parameters = new { CurrentDate = currentDate };

            return _connection.Query<FlightModel>(sql, parameters).ToList();
        }
    }
}