using Spectre.Console;

public static class AdminUI
{
    private static readonly Style primaryStyle = new(new Color(134, 64, 0));      // Deep Brown #864000
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));   // Vivid Orange #FF7A00  
    private static readonly Style errorStyle = new(new Color(162, 52, 0));        // Chestnut Red #A23400
    private static readonly Style successStyle = new(new Color(194, 87, 0));

    public static void ShowUserManagementMenu()
    {
        while (SessionManager.IsLoggedIn() && SessionManager.CurrentUser.IsAdmin)

        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("User Management").Centered().Color(Color.Orange1));

            var choices = new List<string>
            {
                "Register new user",
                "Edit existing user",
                "View admin action logbook",
                "Back to main menu"
            };

            var input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a user management option:[/]")
                    .PageSize(5)
                    .AddChoices(choices)
                    .WrapAround(true));

            switch (input)
            {
                case "Register new user":
                    AdminRegisterUser();
                    break;
                case "Edit existing user":
                    AdminEditUser();
                    break;
                case "View admin action logbook":
                    ViewAdminLogbook();
                    break;
                case "Back to main menu":
                    return;
            }
            
            AnsiConsole.MarkupLine("[yellow]Returning to main menu...[/]");

        }
    }

    public static void ShowFlightManagementMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Flight Management").Centered().Color(Color.Orange1));

            var choices = new List<string>
            {
                "Add a flight",
                "Edit a flight",
                "Remove a flight",
                "View upcoming flights",
                "View past flights",
                "Back to main menu"
            };

            var input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a flight management option:[/]")
                    .PageSize(6)
                    .AddChoices(choices)
                    .WrapAround(true));

            switch (input)
            {
                case "Add a flight":
                    FlightUI.AddFlight();
                    break;
                case "Edit a flight":
                    FlightUI.EditFlight();
                    break;
                case "Remove a flight":
                    FlightUI.RemoveFlight();
                    break;
                case "View upcoming flights":
                    FlightUI.DisplayFilteredFlights();
                    break;
                case "View past flights":
                    FlightUI.DisplayPastFlights();
                    break;
                case "Back to main menu":
                    return;
            }
        }
    }

    public static void AdminRegisterUser()
    {
        if (!SessionManager.IsLoggedIn() || !SessionManager.CurrentUser.IsAdmin)
        {
            var errorPanel = new Panel("[rgb(162,52,0)]You must be logged in as an admin to register new users.[/]")
                .Border(BoxBorder.Double)
                .BorderStyle(errorStyle);
            AnsiConsole.Write(errorPanel);

            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        bool registrationSuccess = false;
        do
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[#FF7A00]Admin User Registration[/]").RuleStyle(primaryStyle));
            AnsiConsole.WriteLine();

            string firstName = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter first name:[/]")
                    .PromptStyle(highlightStyle));

            string lastName = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter last name:[/]")
                    .PromptStyle(highlightStyle));

            string country = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter country:[/]")
                    .PromptStyle(highlightStyle));

            string city = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter city:[/]")
                    .PromptStyle(highlightStyle));

            string email = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter email address:[/]")
                    .PromptStyle(highlightStyle));

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter password:[/]")
                    .Secret()
                    .PromptStyle(highlightStyle));

            string phoneNumberString = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter phone number:[/]")
                    .PromptStyle(highlightStyle));

            string birthDateString = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter birth date (yyyy-mm-dd):[/]")
                    .PromptStyle(highlightStyle));

            UserRole role = AnsiConsole.Prompt(
                new SelectionPrompt<UserRole>()
                    .Title("[#864000]Select user role:[/]")
                    .PageSize(3)
                    .AddChoices(UserRole.Admin, UserRole.Customer));

            DateTime accCreatedAt = DateTime.Now;

            registrationSuccess = UserLogic.Register(firstName, lastName, country, city, email, password, phoneNumberString, birthDateString, accCreatedAt, role);

            if (!registrationSuccess)
            {
                if (UserLogic.errors.Count > 0)
                {
                    var escapedErrors = string.Join("\n", UserLogic.errors.Select(Spectre.Console.Markup.Escape));
                    var panel = new Panel(escapedErrors)
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = errorStyle,
                        Padding = new Padding(1)
                    };
                    AnsiConsole.Write(panel);
                    UserLogic.errors.Clear();
                    AnsiConsole.WriteLine();

                    AnsiConsole.MarkupLine("\n[grey]Press any key to try again or ESC to exit...[/]");
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
            }
            else
            {
                var successPanel = new Panel($"[#FFD58A]User {firstName} {lastName} registered successfully as a {role} user![/]")
                {
                    Border = BoxBorder.Double,
                    BorderStyle = successStyle,
                    Padding = new Padding(1)
                };
                AnsiConsole.Write(successPanel);

                User newUser = UserLogic.GetUserByEmail(email);
                
                try
                {
                    LoggerLogic.LogUserCreation(SessionManager.CurrentUser, newUser);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error logging user creation: {Markup.Escape(ex.Message)}[/]");
                }

                AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
            }
        } while (!registrationSuccess);
    }

    public static void AdminEditUser()
    {   
        if (!SessionManager.IsLoggedIn() || !SessionManager.CurrentUser.IsAdmin)
        {
            var errorPanel = new Panel("[rgb(162,52,0)]You must be logged in as an admin to edit users.[/]")
                .Border(BoxBorder.Double)
                .BorderStyle(errorStyle);
            AnsiConsole.Write(errorPanel);
            
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Admin Edit User[/]").RuleStyle(primaryStyle));
        AnsiConsole.WriteLine();

        string filterOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[#864000]Filter users by:[/]")
                .PageSize(4)
                .AddChoices(new[] { "All Users", "Email", "Name", "Role" }));

        List<User> filteredUsers = new List<User>();
        
        switch (filterOption)
        {
            case "All Users":
                filteredUsers = UserLogic.GetAllUsers();
                break;
                
            case "Email":
                string emailFilter = AnsiConsole.Prompt(
                    new TextPrompt<string>("[#864000]Enter email (partial match):[/]")
                        .PromptStyle(highlightStyle));
                filteredUsers = UserLogic.GetUsersByEmail(emailFilter);
                break;
                
            case "Name":
                string nameFilter = AnsiConsole.Prompt(
                    new TextPrompt<string>("[#864000]Enter name (partial match):[/]")
                        .PromptStyle(highlightStyle));
                filteredUsers = UserLogic.GetUsersByName(nameFilter);
                break;
                
            case "Role":
                var filterRole = AnsiConsole.Prompt(
                    new SelectionPrompt<UserRole>()
                        .Title("[#864000]Filter by user role:[/]")
                        .PageSize(4)
                        .AddChoices(UserRole.Admin, UserRole.Customer));
                
                filteredUsers = UserLogic.GetUsersByRole(filterRole);
                break;
        }

        if (filteredUsers.Count == 0)
        {
            var panel = new Panel("[yellow]No users found matching your criteria.[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(errorStyle);
            AnsiConsole.Write(panel);
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(primaryStyle)
            .Expand();

        table.AddColumns(
            "[#864000]ID[/]", "[#864000]Name[/]", "[#864000]Email[/]",
            "[#864000]Phone[/]", "[#864000]Location[/]", "[#864000]Role[/]"
        );

        foreach (var user in filteredUsers)
        {
            table.AddRow(
                user.UserID.ToString(),
                $"{user.FirstName} {user.LastName}",
                user.Email,
                user.PhoneNumber,
                $"{user.City}, {user.Country}",
                user.Role.ToString()
            );
        }

        AnsiConsole.Write(table);
        
        var userId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter ID of user to edit:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => 
                    filteredUsers.Any(u => u.UserID == id) 
                        ? ValidationResult.Success() 
                        : ValidationResult.Error("[red]Invalid user ID[/]")));
        
        User selectedUser = filteredUsers.First(u => u.UserID == userId);
        
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Edit User Details[/]").RuleStyle(primaryStyle));
        AnsiConsole.WriteLine();
        
        selectedUser.FirstName = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new first name[/]")
                .DefaultValue(selectedUser.FirstName)
                .PromptStyle(highlightStyle));
        
        selectedUser.LastName = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new last name[/]")
                .DefaultValue(selectedUser.LastName)
                .PromptStyle(highlightStyle));
        
        selectedUser.Email = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new email[/]")
                .DefaultValue(selectedUser.Email)
                .PromptStyle(highlightStyle));
        
        selectedUser.PhoneNumber = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new phone number[/]")
                .DefaultValue(selectedUser.PhoneNumber)
                .PromptStyle(highlightStyle));
        
        selectedUser.City = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new city[/]")
                .DefaultValue(selectedUser.City)
                .PromptStyle(highlightStyle));
        
        selectedUser.Country = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter new country[/]")
                .DefaultValue(selectedUser.Country)
                .PromptStyle(highlightStyle));

        selectedUser.Role = AnsiConsole.Prompt(
            new SelectionPrompt<UserRole>()
                .Title("[#864000]Select new role[/]")
                .PageSize(4)
                .HighlightStyle(highlightStyle)
                .AddChoices(UserRole.Customer, UserRole.Admin));

        // Save changes
        bool updateSuccess = UserLogic.UpdateUser(selectedUser);
        
        if (updateSuccess)
        {
            AnsiConsole.MarkupLine("[green]User updated successfully![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to update user.[/]");
            
            if (UserLogic.errors.Count > 0)
            {
                var escapedErrors = string.Join("\n", UserLogic.errors.Select(Spectre.Console.Markup.Escape));
                var errorPanel = new Panel(escapedErrors)
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = errorStyle,
                    Padding = new Padding(1)
                };
                AnsiConsole.Write(errorPanel);
                UserLogic.errors.Clear();
            }
        }
        
        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    public static void ViewAdminLogbook()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[#FF7A00]Admin Action Logbook[/]").RuleStyle(primaryStyle));
        AnsiConsole.WriteLine();

        try
        {
            List<LogEntry> logEntries = LoggerLogic.ReadLogEntries();
            
            if (logEntries.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No logbook entries found.[/]");
                AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderStyle(primaryStyle)
                .Expand();

            table.AddColumns(
                "[#864000]Time[/]", 
                "[#864000]Action[/]", 
                "[#864000]Admin[/]", 
                "[#864000]Target User[/]", 
                "[#864000]Details[/]"
            );

            foreach (var entry in logEntries)
            {
                string details = entry.Details?.Replace("|", "\n") ?? "";

                table.AddRow(
                    entry.Timestamp,
                    entry.Action,
                    entry.AdminName,
                    entry.TargetUserName,
                    details
                );
                
                if (entry != logEntries.Last())
                {
                    table.AddEmptyRow();
                }
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            var errorPanel = new Panel($"[red]Error reading log entries: {Markup.Escape(ex.Message)}[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(errorStyle);
            AnsiConsole.Write(errorPanel);
        }
        
        AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
        Console.ReadKey(true);
    }
}