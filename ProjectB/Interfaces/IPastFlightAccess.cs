
public interface IPastFlightAccess
{
    void DeletePastFlights(DateTime monthAgo);
    void WritePastFlight(FlightModel flight);
}

