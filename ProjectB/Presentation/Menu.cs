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
                    "View user info",
                    "Logout"
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
                    "Search for flights",
                    "Reviews",
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
                    UserUI.ShowGuestMenu();
                    break;

                case "Login":
                    SessionManager.Logout();
                    UserUI.UserLogin();
                    break;

                case "Register":
                    UserUI.RegisterAccount();
                    break;

                case "Logout":
                    SessionManager.Logout();
                    break;

                case "Book a flight":
                    BookingUI.BookADamnFlight();
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

                case "Search for flights":
                    FlightUI.SearchFlights();
                    break;

                case "Reviews":
                    ReviewUI.ShowReviewMenu();
                    break;
                
            }
        }
    }
}