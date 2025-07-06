public class User {
    public int UserID { get; set; } // Primary Key
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime AccCreatedAt { get; set; }
    public UserRole Role { get; set; }
    public bool FirstTimeDiscount { get; set; } // Default to true, can be set to false after first use
    // Not in database, but used for logic
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsGuest => Role == UserRole.Guest;
    public bool IsCustomer => Role == UserRole.Customer;

    public User() { }

    public User(
        int userID,
        string firstName,
        string lastName,
        string country,
        string city,
        string emailAddress,
        string password,
        string phoneNumber,
        DateTime birthDate,
        DateTime accCreatedAt,
        UserRole role = UserRole.Customer,
        bool firstTimeDiscount = true
    )
    {
        UserID = userID;
        FirstName = firstName;
        LastName = lastName;
        Country = country;
        City = city;
        Email = emailAddress;
        Password = password;
        PhoneNumber = phoneNumber;
        BirthDate = birthDate;
        AccCreatedAt = accCreatedAt;
        Role = role;
        FirstTimeDiscount = firstTimeDiscount;
    }
}