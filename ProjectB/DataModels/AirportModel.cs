public class AirportModel
{
    public string IataCode { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    public AirportModel(string iataCode, string name, string city, string country)
    {
        IataCode = iataCode;
        Name = name;
        City = city;
        Country = country;
    }
}