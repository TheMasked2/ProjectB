using Spectre.Console;
using ProjectB.DataAccess;
public static class FlightLogic
{
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    /// <summary>
    /// Retrieves all flights.
    /// </summary>
    /// <returns>A list of all flights.</returns>
    // public static List<FlightModel> GetAllFlights()
    // {
    //     return FlightAccess.GetAllFlightData();
    // }

    /// <summary>
    /// Filters the data from the data access layer based on the provided criteria.
    /// </summary>
    /// <param name="minPrice">Minimum price filter.</param>
    /// <param name="maxPrice">Maximum price filter.</param>
    /// <param name="startDate">Start date filter.</param>
    /// <param name="endDate">End date filter.</param>
    /// <param name="origin">Origin airport filter.</param>
    /// <param name="destination">Destination airport filter.</param>
    /// <returns></returns>
    public static List<FlightModel> GetFilteredFlights(
        string origin,
        string destination, 
        DateTime startDate,
        DateTime endDate,
        int? minPrice,
        int? maxPrice,
        string? seatClass)
    {
        if (string.IsNullOrEmpty(origin))
        {
            throw new ArgumentException("Origin cannot be null or empty.");
        }
        if (string.IsNullOrEmpty(destination))
        {
            throw new ArgumentException("Destination cannot be null or empty.");
        }

        var flights = FlightAccessService.GetAllFlightData();

        // Mandatory filters
        flights = flights.Where(f => f.DepartureAirport.Equals(origin, StringComparison.OrdinalIgnoreCase)).ToList();
        flights = flights.Where(f => f.ArrivalAirport.Equals(destination, StringComparison.OrdinalIgnoreCase)).ToList();
        flights = flights.Where(f => f.DepartureTime.Date >= startDate.Date).ToList();
        flights = flights.Where(f => f.DepartureTime.Date <= endDate.Date).ToList();

        // Seat class filter (only apply if seatClass is not null or empty)
        // if (!string.IsNullOrEmpty(seatClass))
        // {
        //     flights = flights.Where(f => f.AirplaneID.Equals(seatClass, StringComparison.OrdinalIgnoreCase)).ToList();
        // }

        // Optional price filters
        flights = flights.Where(f => !minPrice.HasValue || f.Price >= minPrice.Value).ToList();
        flights = flights.Where(f => !maxPrice.HasValue || f.Price <= maxPrice.Value).ToList();

        return flights.ToList();
    }

    /// <summary>
    /// Retrieves a flight by its ID.
    /// </summary>
    /// <param name="flightId">The ID of the flight to retrieve.</param>
    /// <returns>The flight with the specified ID, or null if not found.</returns>
    public static FlightModel GetFlightById(int flightId)
    {
        if (flightId <= 0)
        {
            throw new ArgumentException("FlightModel ID must be greater than zero.");
        }

        return FlightAccessService.GetById(flightId);
    }

    /// <summary>
    /// Adds a new flight after validating its details.
    /// </summary>
    /// <param name="flight">The flight to add.</param>
    public static bool AddFlight(FlightModel flight)
    {
        try
        {
            ValidateFlight(flight);

            // Set default values for required fields
            AirplaneModel airplane = AirplaneLogic.GetAllAirplanes(flight.AirplaneID);

            if (airplane == null)
            {
                throw new ArgumentException("Airplane not found.");
            }
            flight.AirplaneID = airplane.AirplaneID;
            flight.AvailableSeats = airplane.TotalSeats;

            flight.FlightStatus = "Scheduled";
            
            // Get next available ID
            AutoIncrementFlightID(flight);
            
            FlightAccessService.Write(flight);

            // Initialize seat occupancy for this flight
            FlightSeatAccessService.CreateFlightSeats(flight.FlightID, flight.AirplaneID);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding flight: {ex.Message}");
            return false;
        }
    }

    // Temp fix for the autoincrement
    /// <summary>
    /// Assigns the next available flight ID to a new flight.
    /// This is a temporary solution until proper auto-increment is implemented in the database.
    /// </summary>
    /// <param name="flight">The flight to assign an ID to.</param>
    private static void AutoIncrementFlightID(FlightModel flight)
    {
        var existingFlights = FlightAccessService.GetAllFlightData();

        int nextId = 1;

        // If flights exist, find highest ID and add 1
        if (existingFlights.Count > 0)
        {
            nextId = existingFlights.Max(f => f.FlightID) + nextId;
        }

        flight.FlightID = nextId;
    }

    /// <summary>
    /// Updates an existing flight after validating its details.
    /// </summary>
    /// <param name="flight">The flight to update.</param>
    public static bool UpdateFlight(FlightModel flight)
    {
        if (flight.FlightID <= 0)
        {
            throw new ArgumentException("FlightModel ID must be valid for updates.");
        }

        ValidateFlight(flight);
        try
        {
            FlightAccessService.Update(flight);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating flight: {ex.Message}");
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Deletes a flight by its ID.
    /// </summary>
    /// <param name="flightId">The ID of the flight to delete.</param>
    public static bool DeleteFlight(int flightId)
    {
        if (flightId <= 0)
        {
            // throw new ArgumentException("FlightModel ID must be greater than zero.");
            return false;
        }

        FlightAccessService.Delete(flightId);
        return true;
    }

    /// <summary>
    /// Validates the details of a flight.
    /// </summary>
    /// <param name="flight">The flight to validate.</param>
    private static void ValidateFlight(FlightModel flight)
    {
        var today = DateTime.Today;

        if (string.IsNullOrWhiteSpace(flight.Airline))
        {
            throw new ArgumentException("Airline name cannot be empty.");
        }

        if (flight.DepartureTime.Date < today.Date)
        {
            throw new ArgumentException("Departure date cannot be in the past.");
        }

        if (flight.DepartureTime >= flight.ArrivalTime)
        {
            throw new ArgumentException("Departure time must be earlier than arrival time.");
        }

        if (flight.Price < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        if (flight.AvailableSeats < 0)
        {
            throw new ArgumentException("Available seats cannot be negative.");
        }
    }

}