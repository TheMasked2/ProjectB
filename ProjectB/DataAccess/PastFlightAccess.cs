using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

namespace ProjectB.DataAccess
{
    public class PastFlightAccess : IPastFlightAccess
    {
        private readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
        private const string Table = "PASTFLIGHTS";

        public void DeleteOldPastFlights(List<int> flightIDs)
        {
            string sql = $@"DELETE FROM {Table} WHERE FlightID IN @FlightIDs";
            var parameters = new { FlightIDs = flightIDs };
            _connection.Execute(sql, parameters);
        }

        public List<int> GetOldPastFlightIDs(DateTime monthAgo)
        {
            string sql = $"SELECT FlightID FROM {Table} WHERE DepartureTime < @MonthAgoTime";
            var parameters = new { MonthAgoTime = monthAgo };
            List<int> flightIDs = _connection.Query<int>(sql, parameters).ToList();
            return flightIDs;
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

        public List<FlightModel> GetAllPastFlights()
        {
            string sql = $@"SELECT * FROM {Table}";
            return _connection.Query<FlightModel>(sql).ToList();
        }

        public List<FlightModel> GetFilteredPastFlights(
            string? origin,
            string? destination,
            DateTime departureDate)
        {
            string sql = $@"SELECT * FROM {Table}
                            WHERE DepartureTime = @DepartureDate
                            AND DepartureAirport LIKE @Origin
                            AND ArrivalAirport LIKE @Destination";
            
            var parameters = new
            {
                DepartureDate = departureDate.Date,
                Origin = string.IsNullOrEmpty(origin) ? "%" : origin,
                Destination = string.IsNullOrEmpty(destination) ? "%" : destination
            };

            return _connection.Query<FlightModel>(sql, parameters).ToList();
        }
    }
}