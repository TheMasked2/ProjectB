using Spectre.Console;

public static class Menu
{
    public static void ShowMainMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Airtreides Booking")
                    .Centered()
                    .Color(Color.Cyan1));
    
            var choices = new List<string>();
    
            if (SessionManager.CurrentUser == null)
            {
                choices.AddRange(new[]
                {
                    "Login",
                    "Register",
                    "Guest",
                    "Exit"
                });
            }
            else if (SessionManager.CurrentUser.IsAdmin)
            {
                choices.AddRange(new[]
                {
                    "Flight management",
                    "User management",
                    "View all flights",
                    "View past flights",
                    "View user info",
                    "Logout"
                });
            }
            else if (SessionManager.CurrentUser.Guest)
            {
                choices.AddRange(new[]
                {
                    "Login",
                    "Register",
                    "Guest",
                    "Exit"
                });
            }
            else
            {
                choices.AddRange(new[]
                {
                    "Book a flight",
                    "View bookings",
                    "View user info",
                    "Edit user info",
                    "Logout"
                });
            }
    
            var input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select an option:[/]")
                    .PageSize(10)
                    .AddChoices(choices));

            switch (input)
            {
                case "Exit" when SessionManager.CurrentUser == null:
                    Console.Clear();
                    Environment.Exit(0);
                    break;
                
                case "Guest":
                    AnsiConsole.Clear();
                    BookingUI.DisplayAllBookableFlights();
                    Console.ReadKey();
                    SessionManager.CurrentUser = new User
                    {
                        UserID = 0,
                        FirstName = "Guest",
                        LastName = "User",
                        IsAdmin = false,
                        Guest = true
                    };
                    break;

                case "Login":
                    UserUI.UserLogout();
                    UserUI.UserLogin();
                    break;

                case "Register":
                    UserUI.RegisterAccount();
                    break;

                case "Logout":
                    UserUI.UserLogout();
                    break;

                case "Book a flight":
                    BookingUI.DisplayAllBookableFlights();
                    break;

                case "View all flights" when SessionManager.CurrentUser?.IsAdmin == true:
                    FlightUI.DisplayAllFlights();
                    break;

                case "View past flights" when SessionManager.CurrentUser?.IsAdmin == true:
                    PastFlightUI.DisplayAllFlights();
                    break;

                case "Flight management" when SessionManager.CurrentUser?.IsAdmin == true:
                    AdminUI.ShowFlightManagementMenu();
                    break;

                case "User management" when SessionManager.CurrentUser?.IsAdmin == true:
                    AdminUI.ShowUserManagementMenu();
                    break;

                case "View user info":
                    UserUI.DisplayUserInfo();
                    break;

                case "View bookings":
                    UserUI.ShowBookingMenu();
                    break;

                case "Edit user info":
                    UserUI.UserEditUser();
                    break;
            }
        }
    }
}