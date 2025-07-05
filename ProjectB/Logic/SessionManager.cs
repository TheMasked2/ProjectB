public static class SessionManager
{
    public static User? CurrentUser { get; private set; }
    public static DateTime LoginTime { get; private set; }

    public static void SetCurrentUser(User user)
    {
        CurrentUser = user;
        LoginTime = DateTime.Now;
    }
    public static void SetGuestUser()
    {
        User guest = new User
        {
            UserID = -1,
            FirstName = "Guest",
            LastName = "User",
            Role = UserRole.Guest
        };
        CurrentUser = guest;
        LoginTime = DateTime.Now;
    }
    public static void Logout()
    {
        CurrentUser = null;
        LoginTime = DateTime.MinValue;
    }

    public static bool IsLoggedIn()
    {
        return CurrentUser != null;
    }
}