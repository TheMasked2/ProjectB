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
    public bool FirstTimeDiscount { get; set; }
    // Not in database, but used for logic:
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsGuest => Role == UserRole.Guest;
    public bool IsCustomer => Role == UserRole.Customer;

    public User() { }
}