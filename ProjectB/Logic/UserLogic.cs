using ProjectB.DataAccess;
public static class UserLogic
{
    public static IUserAccess UserAccessService { get; set; } = new UserAccess();
    public static List<string> errors = new();

    public static bool Register(
        string firstName,
        string lastName,
        string country,
        string city,
        string emailAddress,
        string password,
        string confirmPassword,
        string phoneNumberString,
        string birthDateString
    )
    {
        errors.Clear();

        if (string.IsNullOrWhiteSpace(phoneNumberString) || !phoneNumberString.All(char.IsDigit))
        {
            errors.Add("Phone number must contain only digits and cannot be empty.");
        }

        if (!DateTime.TryParse(birthDateString, out DateTime birthDate))
        {
            errors.Add("Birth date is not in the correct format (yyyy-mm-dd).");
        }

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(country) ||
            string.IsNullOrEmpty(city) || string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(password))
        {
            errors.Add("All fields are required.");
        }

        if (!CheckEmail(emailAddress))
        {
            errors.Add("Email validation failed.");
        }

        if (password != confirmPassword)
        {
            errors.Add("Passwords do not match.");
        }

        if (errors.Count > 0)
        {
            return false;
        }

        User newUser = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Country = country,
            City = city,
            Email = emailAddress,
            Password = password,
            PhoneNumber = phoneNumberString,
            BirthDate = birthDate,
            AccCreatedAt = DateTime.Now,
            Role = UserRole.Customer,
            FirstTimeDiscount = true
        };

        UserAccessService.Insert(newUser);
        return true;
    }

    // Overloaded Register method for admin use
    public static bool Register(
        string firstName,
        string lastName,
        string country,
        string city,
        string emailAddress,
        string password,
        string phoneNumberString,
        string birthDateString,
        DateTime accCreatedAt,
        UserRole role
    )
    {
        errors.Clear();

        if (string.IsNullOrWhiteSpace(phoneNumberString) || !phoneNumberString.All(char.IsDigit))
        {
            errors.Add("Phone number must contain only digits and cannot be empty.");
        }

        if (!DateTime.TryParse(birthDateString, out DateTime birthDate))
        {
            errors.Add("Birth date is not in the correct format (yyyy-mm-dd).");
        }

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(country) ||
            string.IsNullOrEmpty(city) || string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(password))
        {
            errors.Add("All fields are required.");
        }

        if (!CheckEmail(emailAddress))
        {
            errors.Add("Email validation failed.");
        }

        if (errors.Count > 0)
        {
            return false;
        }

        User newUser = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Country = country,
            City = city,
            Email = emailAddress,
            Password = password,
            PhoneNumber = phoneNumberString,
            BirthDate = birthDate,
            AccCreatedAt = accCreatedAt,
            Role = role,
            FirstTimeDiscount = role == UserRole.Customer
        };

        UserAccessService.Insert(newUser);
        return true;
    }

    public static bool Login(string email, string password)
    {
        if (SessionManager.IsLoggedIn())
        {
            errors.Add("You are already logged in.");
            return false;
        }

        User loggedInUser = UserAccessService.Login(email, password);

        if (loggedInUser != null)
        {
            SessionManager.SetCurrentUser(loggedInUser);
            return true;
        }
        else
        {
            errors.Add("Password or email is incorrect.");
            return false;
        }
    }

    private static bool CheckEmail(string email)
    {
        if (UserAccessService.GetUserInfoByEmail(email) != null)
        {
            errors.Add("Email already exists.");
            return false;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            errors.Add("Email is not valid.");
            return false;
        }

        return true;
    }

    // Had to make public cause of UserEditUser()
    public static bool VerifyPassword(string email, string userPassword)
    {
        var userInfo = UserAccessService.GetUserInfoByEmail(email);

        if (userInfo == null)
        {
            errors.Add("User not found.");
            return false;
        }
        else if (userInfo.Password != userPassword)
        {
            errors.Add("Password is incorrect.");
            return false;
        }
        return true;
    }

    public static List<User> GetAllUsers()
    {
        return UserAccessService.GetAll();
    }

    public static void UpdateGuestUser(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string birthDateStr)
    {
        errors.Clear();

        if (!DateTime.TryParse(birthDateStr, out DateTime birthDate))
        {
            errors.Add("Birth date is not in the correct format (yyyy-mm-dd).");
            return;
        }

        User? guestUser = SessionManager.CurrentUser;

        // Update guest user properties
        guestUser.FirstName = firstName;
        guestUser.LastName = lastName;
        guestUser.Email = email;
        guestUser.PhoneNumber = phoneNumber;
        guestUser.BirthDate = birthDate;
        guestUser.FirstTimeDiscount = false; // Guest users do not get discounts

        SessionManager.SetCurrentUser(guestUser);
    }

    public static bool UpdateUser(User updatedUser)
    {
        errors.Clear();

        // If admin is updating, get original user and prepare to track changes
        bool isAdminUpdate = SessionManager.IsLoggedIn() && SessionManager.CurrentUser.IsAdmin;

        User originalUser = null;
        Dictionary<string, string> changedFields = null;
        if (isAdminUpdate)
        {
            originalUser = UserAccessService.GetById(updatedUser.UserID);
            if (originalUser != null)
            {
                changedFields = new Dictionary<string, string>();

                if (updatedUser.FirstName != originalUser.FirstName)
                    changedFields.Add("FirstName", $"{originalUser.FirstName} -> {updatedUser.FirstName}");

                if (updatedUser.LastName != originalUser.LastName)
                    changedFields.Add("LastName", $"{originalUser.LastName} -> {updatedUser.LastName}");

                if (updatedUser.Email != originalUser.Email)
                    changedFields.Add("Email", $"{originalUser.Email} -> {updatedUser.Email}");

                if (updatedUser.PhoneNumber != originalUser.PhoneNumber)
                    changedFields.Add("PhoneNumber", $"{originalUser.PhoneNumber} -> {updatedUser.PhoneNumber}");

                if (updatedUser.City != originalUser.City)
                    changedFields.Add("City", $"{originalUser.City} -> {updatedUser.City}");

                if (updatedUser.Country != originalUser.Country)
                    changedFields.Add("Country", $"{originalUser.Country} -> {updatedUser.Country}");

                if (originalUser.Role != updatedUser.Role)
                    changedFields.Add("Role", $"{originalUser.Role} -> {updatedUser.Role}");
            }
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(updatedUser.FirstName))
        {
            errors.Add("[#A23400]First name cannot be empty[/]");
        }

        if (string.IsNullOrWhiteSpace(updatedUser.LastName))
        {
            errors.Add("[#A23400]Last name cannot be empty[/]");
        }

        if (string.IsNullOrWhiteSpace(updatedUser.Country))
        {
            errors.Add("[#A23400]Country cannot be empty[/]");
        }

        if (string.IsNullOrWhiteSpace(updatedUser.City))
        {
            errors.Add("[#A23400]City cannot be empty[/]");
        }

        if (string.IsNullOrWhiteSpace(updatedUser.Email))
        {
            errors.Add("[#A23400]Email address cannot be empty[/]");
        }

        if (string.IsNullOrWhiteSpace(updatedUser.Password))
        {
            errors.Add("[#A23400]Password cannot be empty[/]");
        }

        if (string.IsNullOrWhiteSpace(updatedUser.PhoneNumber))
        {
            errors.Add("[#A23400]Phone number cannot be empty[/]");
        }

        // If there are validation errors, return false
        if (errors.Count > 0)
        {
            return false;
        }

        try
        {
            UserAccessService.Update(updatedUser);

            // Log if admin update and if any fields actually changed
            if (isAdminUpdate && originalUser != null && changedFields != null && changedFields.Count > 0)
            {
                LoggerLogic.LogUserEdit(SessionManager.CurrentUser, updatedUser, changedFields);
            }
            // Check if the updated user is the currently logged-in user
            if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.UserID == updatedUser.UserID)
            {
                SessionManager.SetCurrentUser(updatedUser);
            }

            return true;
        }
        catch (Exception ex)
        {
            errors.Add($"[#A23400]Update failed: {ex.Message}[/]");
            return false;
        }
    }

    public static List<User> GetUsersByEmail(string emailFilter)
    {
        emailFilter = emailFilter.ToLower();
        return UserAccessService.GetAll().Where(u =>
            u.Email.ToLower().Contains(emailFilter)).ToList();
    }

    public static List<User> GetUsersByName(string nameFilter)
    {
        nameFilter = nameFilter.ToLower();
        return UserAccessService.GetAll().Where(u =>
            u.FirstName.ToLower().Contains(nameFilter) ||
            u.LastName.ToLower().Contains(nameFilter) ||
            $"{u.FirstName.ToLower()} {u.LastName.ToLower()}".Contains(nameFilter)
        ).ToList();
    }

    public static List<User>? GetUsersByRole(UserRole role)
    {
        return UserAccessService.GetAll().Where(u => u.Role == role).ToList();
    }

    public static User? GetUserByEmail(string email)
    {
        return UserAccessService.GetUserInfoByEmail(email);
    }

    public static User? GetUserByID(int userId)
    {
        return UserAccessService.GetById(userId);
    }
}