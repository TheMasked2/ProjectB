using Spectre.Console;
using ProjectB.DataAccess;
using Microsoft.VisualBasic;
public static class FlightLogic
{
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    public static IAirplaneAccess AirplaneAccessService { get; set; } = new AirplaneAccess();
    public static IPastFlightAccess PastFlightAccessService { get; set; } = new PastFlightAccess();
    public static ISeatAccess SeatAccessService { get; set; } = new SeatAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    /// <summary>
    /// Filters the data from the data access layer based on the provided criteria.
    /// </summary>
    /// <param name="origin">Origin airport filter.</param>
    /// <param name="destination">Destination airport filter.</param>
    /// <param name="departureDate">Departure date filter.</param>
    /// <returns>List of filtered flights.</returns>
    public static List<FlightModel> GetFilteredFlights(
        string? origin,
        string? destination,
        DateTime departureDate) => FlightAccessService.GetFilteredFlights(origin, destination, departureDate);

    public static List<FlightModel> GetFilteredFlights(
        string? origin,
        string? destination,
        DateTime departureDate,
        string seatClass)
    {
        List<FlightModel> flights = FlightAccessService.GetFilteredFlights(origin, destination, departureDate);

        List<FlightModel> bookableFlights =
            flights.Where(flight => 
                FlightSeatAccessService.GetAvailableSeatCountByClass(flight.FlightID, flight.AirplaneID, seatClass) > 0 && 
                GetSeatClassPrice(flight.AirplaneID, seatClass) > 0
            ).ToList();

        return bookableFlights;
    }

    public static Spectre.Console.Rendering.IRenderable CreateDisplayableFlightsTable(List<FlightModel> flights, string seatClass = null)
    {
        if (flights == null || !flights.Any())
        {
            var panel = new Panel("[yellow]No flights found matching the criteria. Please try again.[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(errorStyle);
            return panel;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();

        table.AddColumns(
            "[#864000]ID[/]", "[#864000]Aircraft ID[/]", "[#864000]Airline[/]",
            "[#864000]From[/]", "[#864000]To[/]", "[#864000]Departure[/]",
            "[#864000]Arrival[/]", "[#864000]Status[/]"
        );

        if (seatClass != null)
        {
            table.AddColumn("[#864000]Price[/]");
        }

        foreach (var flight in flights)
        {
            table.AddRow(
                flight.FlightID.ToString(),
                flight.AirplaneID,
                flight.Airline,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureTime.ToString("g"),
                flight.ArrivalTime.ToString("g"),
                flight.FlightStatus
            );

            if (seatClass != null)
            {
                table.AddRow($"€{FlightLogic.GetSeatClassPrice(flight.AirplaneID, seatClass):F2}");
            }
        }

        return table;
    }

    public static FlightModel GetFlightById(int flightId)
    {
        FlightModel flight = FlightAccessService.GetById(flightId);
        if (flight == null)
        {
            return null;
        }
        return flight;
    }

    public static FlightModel GetReviewFlightById(int flightId)
    {
        FlightAccess flightAccess = new FlightAccess();
        try
        {
            FlightModel flight = flightAccess.GetReviewFlightByID(flightId);
            if (flight == null)
            {
                return null;
            }
            return flight;
        }
        catch
        {
            return null; 
        }
    }


    public static bool AddFlight(FlightModel flight)
    {
        try
        {
            ValidateFlight(flight);

            // Set default values for required fields
            AirplaneModel airplane = AirplaneLogic.GetAirplaneByID(flight.AirplaneID);

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

    public static bool DeleteFlight(int flightId)
    {
        if (flightId <= 0)
        {
            return false;
        }

        FlightAccessService.Delete(flightId);
        return true;
    }

    // Helper for AddFlight
    private static void AutoIncrementFlightID(FlightModel flight)
    {
        var existingFlights = FlightAccessService.GetAllFlightData();

        int nextId = 1;

        // If flights exist, find highest ID and add 1
        if (existingFlights.Count > 0)
        {
            nextId = existingFlights.Max(f => f.FlightID) + 1;
        }

        flight.FlightID = nextId;
    }

    // Helper for Add/Update
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

    private static float GetSeatClassPrice(string airplaneID, string seatClass)
    {
        if(seatClass == null)
        {
            return SeatAccessService.GetSeatClassPrice(airplaneID);
        }
        return SeatAccessService.GetSeatClassPrice(airplaneID, seatClass);
    }

    // Grab all flights from yesterday (and before) and update their status to "Departed", then move them to the past flights table.
    public static void UpdateFlightDB()
    {
        DateTime currentDate = DateTime.Now;
        DateTime monthAgo = currentDate.AddMonths(-1);

        // Update flights that have already departed
        List<FlightModel> pastFlights = FlightAccessService.GetPastFlights(currentDate);
        foreach (var flight in pastFlights)
        {
            // Update flight
            flight.FlightStatus = "Departed";
            FlightAccessService.Update(flight);
            // Move to past flights
            PastFlightAccessService.WritePastFlight(flight);
            // Remove from current flights
            FlightAccessService.Delete(flight.FlightID);
        }
        // Remove past flights and their seats which are older than a month
        PurgeOldPastFlights(monthAgo);

        // Update flights that are departing soon (3 hours or less)
        DateTime departingSoonDate = currentDate.AddHours(3);
        List<FlightModel> upcomingFlights = FlightAccessService.GetUpcomingFlights(departingSoonDate);
        foreach (var flight in upcomingFlights)
        {
            // Update flight status to "Boarding"
            flight.FlightStatus = "Boarding";
            FlightAccessService.Update(flight);
        }
        return;
    }

    private static void PurgeOldPastFlights(DateTime monthAgo)
    {
        List<int> oldFlightIDs = PastFlightAccessService.GetOldPastFlightIDs(monthAgo);
        // Only call if there are any flights to delete
        if (oldFlightIDs != null && oldFlightIDs.Count != 0)
        {
            // Delete seats and flights
            FlightSeatAccessService.DeletePastFlightSeatsByFlightIDs(oldFlightIDs);
            PastFlightAccessService.DeleteOldPastFlights(oldFlightIDs);
        }
        return;
    }
}