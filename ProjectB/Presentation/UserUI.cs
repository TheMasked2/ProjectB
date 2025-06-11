using Spectre.Console;

public static class UserUI
{
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));      // Deep Brown #864000
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));   // Vivid Orange #FF7A00  
    private static readonly Style errorStyle = new(new Color(162, 52, 0));        // Chestnut Red #A23400
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static void RegisterAccount()
    {
        bool registrationSuccess = false;
        do
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[#FF7A00]Welcome to Registration[/]").RuleStyle(primaryStyle));
            AnsiConsole.WriteLine();

            string firstName = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your first name:[/]")
                    .PromptStyle(highlightStyle));

            string lastName = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your last name:[/]")
                    .PromptStyle(highlightStyle));

            string country = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your country:[/]")
                    .PromptStyle(highlightStyle));

            string city = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your city:[/]")
                    .PromptStyle(highlightStyle));

            string email = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your email address:[/]")
                    .PromptStyle(highlightStyle));

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your password:[/]")
                    .Secret()
                    .PromptStyle(highlightStyle));

            string phoneNumberString = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your phone number:[/]")
                    .PromptStyle(highlightStyle));

            string birthDateString = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter your birth date (yyyy-mm-dd):[/]")
                    .PromptStyle(highlightStyle));

            registrationSuccess = UserLogic.Register(firstName, lastName, country, city, email, password, phoneNumberString, birthDateString);

            if (!registrationSuccess)
            {
                DisplayErrors(UserLogic.errors);
                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
            else
            {
                DisplaySuccess(firstName, lastName);
            }
        } while (!registrationSuccess);
    }

    private static void DisplayErrors(List<string> errors)
    {
        var escapedErrors = string.Join("\n", errors.Select(Spectre.Console.Markup.Escape));
        var panel = new Panel(escapedErrors)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = errorStyle,
            Padding = new Padding(1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("\n[grey]Press any key to try again or ESC to exit...[/]");
    }

    private static void DisplaySuccess(string firstName, string lastName)
    {
        var successPanel = new Panel($"[#FFD58A]Welcome {firstName} {lastName}![/]\n[#FFEFCF]Registration successful![/]")
        {
            Border = BoxBorder.Double,
            BorderStyle = successStyle,
            Padding = new Padding(1)
        };
        AnsiConsole.Write(successPanel);
        AnsiConsole.MarkupLine("[#FFD58A]You can now log in with your credentials.[/]");
        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    public static void UserLogin()
    {
        bool loginSuccess = false;
        do
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Airtreides booking")
                    .Centered()
                    .Color(Color.Cyan1));

            var panel = new Panel("[#FFD58A]Please enter your login credentials[/]")
                .Border(BoxBorder.Rounded)
                .Header("[#864000]Login[/]", Justify.Center)
                .Padding(1, 1, 1, 1);
            AnsiConsole.Write(panel);

            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter your email:[/]")
                    .PromptStyle(highlightStyle)
                    .ValidationErrorMessage("[#A23400]Email cannot be empty[/]")
                    .Validate(input => !string.IsNullOrWhiteSpace(input)));

            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter your password:[/]")
                    .Secret()
                    .PromptStyle(highlightStyle)
                    .ValidationErrorMessage("[#A23400]Password cannot be empty[/]")
                    .Validate(input => !string.IsNullOrWhiteSpace(input)));

            loginSuccess = UserLogic.Login(email, password);

            if (loginSuccess)
            {
                AnsiConsole.MarkupLine("[#C25700]Login successful![/]");
                AnsiConsole.MarkupLine($"[#FFD58A]Welcome {SessionManager.CurrentUser.FirstName} {SessionManager.CurrentUser.LastName}[/]");
            }
            else
            {
                var errorPanel = new Panel(string.Join("\n", UserLogic.errors))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = errorStyle,
                    Padding = new Padding(1)
                };
                AnsiConsole.Write(errorPanel);
                UserLogic.errors.Clear();

                AnsiConsole.MarkupLine("\n[grey]Press any key to try again or ESC to exit...[/]");
                try
                {
                    var key = Console.ReadKey(true);
                    if (key != null && key.Key == ConsoleKey.Escape)
                    {
                        loginSuccess = true;
                    }
                }
                catch (InvalidOperationException)
                {
                    loginSuccess = true;
                }
            }
        } while (!loginSuccess);
    }

    public static void UserLogout()
    {
        SessionManager.Logout();
    }

    public static void UserEditUser()
    {
        if (!SessionManager.IsLoggedIn())
        {
            var errorPanel = new Panel("[rgb(162,52,0)]You must be logged in to edit your profile.[/]")
                .Border(BoxBorder.Double)
                .BorderStyle(errorStyle);
            AnsiConsole.Write(errorPanel);

            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Edit Your Profile[/]").RuleStyle(primaryStyle));
        AnsiConsole.WriteLine();

        User currentUser = SessionManager.CurrentUser;

        DisplayUserInfo();
        AnsiConsole.WriteLine();

        // Create a copy of the user
        User editedUser = new User
        {
            UserID = currentUser.UserID,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            EmailAddress = currentUser.EmailAddress,  // Can't be changed
            Password = currentUser.Password,
            Country = currentUser.Country,
            City = currentUser.City,
            PhoneNumber = currentUser.PhoneNumber,
            BirthDate = currentUser.BirthDate,        // Can't be changed
            AccCreatedAt = currentUser.AccCreatedAt,  // Can't be changed
            IsAdmin = currentUser.IsAdmin             // Can't be changed
        };

        // Edit personal information fields
        editedUser.FirstName = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]First name:[/]")
                .DefaultValue(currentUser.FirstName)
                .PromptStyle(highlightStyle));

        editedUser.LastName = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Last name:[/]")
                .DefaultValue(currentUser.LastName)
                .PromptStyle(highlightStyle));

        editedUser.PhoneNumber = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Phone number:[/]")
                .DefaultValue(currentUser.PhoneNumber)
                .PromptStyle(highlightStyle));

        editedUser.City = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]City:[/]")
                .DefaultValue(currentUser.City)
                .PromptStyle(highlightStyle));

        editedUser.Country = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Country:[/]")
                .DefaultValue(currentUser.Country)
                .PromptStyle(highlightStyle));

        bool changePassword = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[#864000]Do you want to change your password?[/]")
                .PageSize(3)
                .HighlightStyle(highlightStyle)
                .AddChoices(new[] { "No", "Yes" })) == "Yes";

        if (changePassword)
        {
            string currentPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter your current password:[/]")
                    .Secret()
                    .PromptStyle(highlightStyle));

            if (UserLogic.VerifyPassword(currentUser.EmailAddress, currentPassword))
            {
                bool passwordsMatch = false;
                do
                {
                    string newPassword = AnsiConsole.Prompt(
                        new TextPrompt<string>("[#864000]Enter new password:[/]")
                            .Secret()
                            .PromptStyle(highlightStyle));

                    string confirmPassword = AnsiConsole.Prompt(
                        new TextPrompt<string>("[#864000]Confirm new password:[/]")
                            .Secret()
                            .PromptStyle(highlightStyle));

                    if (newPassword == confirmPassword)
                    {
                        editedUser.Password = newPassword;
                        passwordsMatch = true;
                    }
                    else
                    {
                        var passwordMismatchPanel = new Panel("[rgb(162,52,0)]Passwords do not match. Please try again.[/]")
                            .Border(BoxBorder.Rounded)
                            .BorderStyle(errorStyle);
                        AnsiConsole.Write(passwordMismatchPanel);

                        AnsiConsole.MarkupLine("\n[grey]Press any key to try again or ESC to exit...[/]");
                        try
                        {
                            var key = Console.ReadKey(true);
                            if (key != null && key.Key == ConsoleKey.Escape)
                            {
                                passwordsMatch = true;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            passwordsMatch = true;
                        }
                    }
                } while (!passwordsMatch);
            }
            else
            {
                var wrongPasswordPanel = new Panel("[rgb(162,52,0)]Incorrect current password. Password will not be changed.[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(errorStyle);
                AnsiConsole.Write(wrongPasswordPanel);
            }
        }

        // Confirm changes
        bool confirmChanges = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[#864000]Save changes to your profile?[/]")
                .PageSize(3)
                .HighlightStyle(highlightStyle)
                .AddChoices(new[] { "Yes", "No" })) == "Yes";

        if (!confirmChanges)
        {
            var cancelPanel = new Panel("[yellow]Profile update canceled.[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(primaryStyle);
            AnsiConsole.Write(cancelPanel);

            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        // Save changes
        bool updateSuccess = UserLogic.UpdateUser(editedUser);

        if (updateSuccess)
        {
            // Update the session with the new user data
            SessionManager.SetCurrentUser(editedUser);

            var successPanel = new Panel("[#FFD58A]Your profile has been updated successfully![/]")
                .Border(BoxBorder.Double)
                .BorderStyle(successStyle);
            AnsiConsole.Write(successPanel);
        }
        else
        {
            var errorPanel = new Panel("[rgb(162,52,0)]Failed to update your profile.[/]")
                .Border(BoxBorder.Double)
                .BorderStyle(errorStyle);
            AnsiConsole.Write(errorPanel);

            if (UserLogic.errors.Count > 0)
            {
                var escapedErrors = string.Join("\n", UserLogic.errors.Select(Spectre.Console.Markup.Escape));
                var errorsPanel = new Panel(escapedErrors)
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = errorStyle,
                    Padding = new Padding(1)
                };
                AnsiConsole.Write(errorsPanel);
                UserLogic.errors.Clear();
            }
        }

        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    public static void DisplayUserInfo()
    {
        if (!SessionManager.IsLoggedIn())
        {
            var errorPanel = new Panel("[rgb(162,52,0)]You must be logged in to view user information.[/]")
                .Border(BoxBorder.Double)
                .BorderStyle(new Style(new Color(162, 52, 0)));
            AnsiConsole.Write(errorPanel);
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("User Profile")
                .Centered()
                .Color(new Color(255, 122, 0)));

        var user = SessionManager.CurrentUser;
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(new Color(184, 123, 74)))
            .Expand();
        table.AddColumn(new TableColumn("[rgb(134,64,0)]User Information[/]").Centered());
        var profileData = new Panel($"""
            [rgb(134,64,0)]Name:[/] [rgb(255,122,0)]{user.FirstName} {user.LastName}[/]
            [rgb(134,64,0)]Location:[/] [rgb(255,122,0)]{user.City}, {user.Country}[/]
            [rgb(134,64,0)]Contact:[/] [rgb(255,122,0)]{user.EmailAddress}[/]
            [rgb(134,64,0)]Password:[/] [rgb(255,122,0)]{user.Password}[/]
            [rgb(134,64,0)]Phone:[/] [rgb(255,122,0)]{user.PhoneNumber}[/]
            [rgb(134,64,0)]Birth Date:[/] [rgb(255,122,0)]{user.BirthDate:yyyy-MM-dd}[/]
            [rgb(134,64,0)]Member Since:[/] [rgb(255,122,0)]{user.AccCreatedAt:yyyy-MM-dd}[/]
            [rgb(134,64,0)]Logged in since:[/] [rgb(255,122,0)]{SessionManager.LoginTime}[/]
            [rgb(134,64,0)]Account Type:[/] [rgb(255,122,0)]{(user.IsAdmin ? "Administrator" : "User")}[/]
            """)
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(new Color(184, 123, 74)))
            .Padding(1, 1);
        table.AddRow(profileData);
        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine("\n[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }

    public static void ShowBookingMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Bookings")
                    .Centered()
                    .Color(new Color(255, 122, 0))); // Orange color consistent with other pages

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#864000]Bookings menu:[/]")
                    .AddChoices("View upcoming bookings", "Cancel a booking", "Modify a booking", "View past bookings", "Back to main menu")
            );

            if (action == "View upcoming bookings")
            {
                BookingLogic.ViewUserBookings(true);
                AnsiConsole.Clear();
            }
            else if (action == "Cancel a booking")
            {
                BookingLogic.CancelBookingPrompt();
                AnsiConsole.Clear();
            }
            else if (action == "Modify a booking")
            {
                BookingLogic.ModifyBookingPrompt();
                AnsiConsole.Clear();
            }
            else if (action == "View past bookings")
            {
                BookingLogic.ViewUserBookings(false);
                AnsiConsole.Clear();
            }
            else // Back to main menu
            {
                AnsiConsole.Clear();
                break;
            }
        }
    }


    public static void ShowGuestMenu()
        {
            SessionManager.CurrentUser = new User
                        {
                            UserID = 0,
                            FirstName = "Guest",
                            LastName = "User",
                            IsAdmin = false,
                            Guest = true
                        };
            
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new FigletText("Guest Menu").Centered().Color(Color.Orange1));

                var choices = new List<string>
                {
                    "Book a flight",
                    "Search for flights",
                    "Back to main menu"
                };

                var input = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select an option:[/]")
                        .PageSize(5)
                        .AddChoices(choices));

                switch (input)
                {
                    case "Book a flight":
                        BookingUI.DisplayAllBookableFlights();
                        break;
                    case "Search for flights":
                        FlightUI.SearchFlights();
                        break;
                    case "Back to main menu":
                        UserLogout();
                        break;
                }
                
                if (input == "Back to main menu")
                    break;
                AnsiConsole.MarkupLine("[yellow]Returning to main menu...[/]");
            }
        }
}