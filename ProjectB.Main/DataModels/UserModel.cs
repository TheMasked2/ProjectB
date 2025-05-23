public class User{
    public int UserID { get; set; } // Primary Key
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string EmailAddress { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime AccCreatedAt { get; set; }
    public bool IsAdmin { get; set; }

    public User() {}

    public User(int userID, string firstName, string lastName, string country, string city, string emailAddress, string password, string phoneNumber, DateTime birthDate, DateTime accCreatedAt, bool isAdmin = false)
    {
        UserID = userID;
        FirstName = firstName;
        LastName = lastName;
        Country = country;
        City = city;
        EmailAddress = emailAddress;
        Password = password;
        PhoneNumber = phoneNumber;
        BirthDate = birthDate;
        AccCreatedAt = accCreatedAt;
        IsAdmin = isAdmin;
    }

}