using Spectre.Console;
using ProjectB.DataAccess;
using System.ComponentModel;

public static class FlightLogic
{
    public static IFlightAccess FlightAccessService { get; set; } = new FlightAccess();
    public static IFlightSeatAccess FlightSeatAccessService { get; set; } = new FlightSeatAccess();
    public static IAirplaneAccess AirplaneAccessService { get; set; } = new AirplaneAccess();
    public static ISeatAccess SeatAccessService { get; set; } = new SeatAccess();
    public static IReviewAccess ReviewAccessService { get; set; } = new ReviewAccess();
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));
    private static readonly Style errorStyle = new(new Color(162, 52, 0));

    public static List<FlightModel> GetFilteredFlights(
        string? origin,
        string? destination,
        DateTime departureDate,
        bool past = false) => FlightAccessService.GetFilteredFlights(origin, destination, departureDate, past);

    public static List<FlightModel> GetFilteredFlights(
        string? origin,
        string? destination,
        DateTime departureDate,
        string? seatClass)
    {
        List<FlightModel> flights = FlightAccessService.GetFilteredFlights(origin, destination, departureDate);

        List<FlightModel> bookableFlights =
            flights.Where(flight =>
                FlightSeatAccessService.GetAvailableSeatCountByClass(flight.FlightID, flight.AirplaneID, seatClass) > 0 &&
                GetSeatClassPrice(flight.AirplaneID, seatClass) > 0
            ).ToList();

        return bookableFlights;
    }

    public static List<FlightModel> GetFilteredPastFlights(
        string? origin,
        string? destination,
        DateTime departureDate) => FlightAccessService.GetFilteredFlights(origin, destination, departureDate, past: true);

    public static Spectre.Console.Rendering.IRenderable CreateDisplayableFlightsTable(List<FlightModel> flights, string? seatClass)
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
            "[#864000]From[/]", "[#864000]To[/]", "[#864000]Departure time[/]",
            "[#864000]Arrival time[/]", "[#864000]Status[/]"
        );

        if (seatClass != null)
        {
            table.AddColumn("[#864000]Price[/]");
        }

        foreach (var flight in flights)
        {
            if (seatClass != null)
            {
                table.AddRow(
                flight.FlightID.ToString(),
                flight.AirplaneID,
                flight.Airline,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureTime.ToString("g"),
                flight.ArrivalTime.ToString("g"),
                flight.Status,
                GetSeatClassPrice(flight.AirplaneID, seatClass).ToString("C"));
            }
            else
            {
                table.AddRow(
                flight.FlightID.ToString(),
                flight.AirplaneID,
                flight.Airline,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureTime.ToString("g"),
                flight.ArrivalTime.ToString("g"),
                flight.Status);
            }
            ;
        }

        return table;
    }

    public static FlightModel? GetFlightById(int flightId)
    {
        return FlightAccessService.GetById(flightId);
    }
    public static void AddFlight(FlightModel flight)
    {
        flight.Status = "Scheduled";
        FlightAccessService.Insert(flight);
        flight.FlightID = FlightAccessService.GetFlightIdByDetails(flight);
        SeatMapLogic.CreateSeatMapForFlight(flight.FlightID, flight.AirplaneID);
    }

    public static void UpdateFlight(FlightModel flight)
    {
        FlightAccessService.Update(flight);
    }

    public static void DeleteFlight(int flightId)
    {
        FlightSeatAccessService.DeleteFlightSeatsByFlightID(flightId);
        BookingLogic.DeleteBookingsByFlightId(flightId);
        FlightAccessService.Delete(flightId);
    }

    private static float GetSeatClassPrice(string airplaneID, string seatClass)
    {
        if (seatClass == null)
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
        foreach (FlightModel flight in pastFlights)
        {
            // Update flight
            flight.Status = "Departed";
            FlightAccessService.Update(flight);
        }
        // Remove past flights and their seats which are older than a month
        PurgeOldPastFlights(monthAgo);

        // Update flights that are departing soon (3 hours or less)
        DateTime departingSoonDate = currentDate.AddHours(3);
        List<FlightModel> upcomingFlights = FlightAccessService.GetUpcomingFlights(departingSoonDate);
        foreach (FlightModel flight in upcomingFlights)
        {
            // Update flight status to "Boarding"
            flight.Status = "Boarding";
            FlightAccessService.Update(flight);
        }
    }

    private static void PurgeOldPastFlights(DateTime monthAgo)
    {
        List<int> oldFlightIDs = FlightAccessService.GetOldDepartedFlightIDs(monthAgo);
        // Only call if there are any flights to delete
        if (oldFlightIDs != null && oldFlightIDs.Count != 0)
        {
            // Delete seats, flights, bookings and reviews by ids
            foreach (int flightId in oldFlightIDs)
            {
                FlightSeatAccessService.DeleteFlightSeatsByFlightID(flightId);
                BookingLogic.DeleteBookingsByFlightId(flightId);
                ReviewAccessService.DeleteReviewsByFlightID(flightId);
            }

            FlightSeatAccessService.DeleteFlightSeatsByFlightIDs(oldFlightIDs);
            FlightAccessService.DeleteFlightsByIDs(oldFlightIDs);
        }
    }
}