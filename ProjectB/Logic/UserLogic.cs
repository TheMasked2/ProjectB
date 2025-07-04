public static class UserLogic
{
    public static IUserAccess UserAccessService { get; set; } = new UserAccess();
    public static List<string> errors = new List<string>();

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

        if (!int.TryParse(phoneNumberString, out int phoneNumber))
        {
            errors.Add("Phone number is not a number.");
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

        var user = new User(
            userID: UserAccessService.GetHighestUserId() + 1,
            firstName, lastName, country, city, emailAddress, password,
            phoneNumber.ToString(), birthDate, DateTime.Now, isAdmin: false, firstTimeDiscount: true
        );

        UserAccessService.AddUser(user);
        return true;
    }

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
        bool isAdmin
    )
    {
        errors.Clear();

        if (!int.TryParse(phoneNumberString, out int phoneNumber))
        {
            errors.Add("Phone number is not a number.");
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

        var user = new User(
            userID: UserAccessService.GetHighestUserId() + 1,
            firstName, lastName, country, city, emailAddress, password,
            phoneNumber.ToString(), birthDate, accCreatedAt, isAdmin
        );

        UserAccessService.AddUser(user);

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

        if (loggedInUser == null)
        {
            errors.Add("Password or email is incorrect.");
            return false;
        }

        if (VerifyPassword(email, password))
        {
            SessionManager.SetCurrentUser(loggedInUser);
            return true;
        }

        errors.Add("Password or email is incorrect.");
        return false;
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
        return UserAccessService.GetAllUsers();
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

        User guestUser = SessionManager.CurrentUser;

        // Update guest user properties
        guestUser.FirstName = firstName;
        guestUser.LastName = lastName;
        guestUser.EmailAddress = email;
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
            originalUser = UserAccessService.GetUserById(updatedUser.UserID);
            if (originalUser != null)
            {
                changedFields = new Dictionary<string, string>();

                if (updatedUser.FirstName != originalUser.FirstName)
                    changedFields.Add("FirstName", $"{originalUser.FirstName} -> {updatedUser.FirstName}");

                if (updatedUser.LastName != originalUser.LastName)
                    changedFields.Add("LastName", $"{originalUser.LastName} -> {updatedUser.LastName}");

                if (updatedUser.EmailAddress != originalUser.EmailAddress)
                    changedFields.Add("Email", $"{originalUser.EmailAddress} -> {updatedUser.EmailAddress}");

                if (updatedUser.PhoneNumber != originalUser.PhoneNumber)
                    changedFields.Add("PhoneNumber", $"{originalUser.PhoneNumber} -> {updatedUser.PhoneNumber}");

                if (updatedUser.City != originalUser.City)
                    changedFields.Add("City", $"{originalUser.City} -> {updatedUser.City}");

                if (updatedUser.Country != originalUser.Country)
                    changedFields.Add("Country", $"{originalUser.Country} -> {updatedUser.Country}");

                string originalAdminStatus = originalUser.IsAdmin ? "Admin" : "User";
                string newAdminStatus = updatedUser.IsAdmin ? "Admin" : "User";
                if (originalAdminStatus != newAdminStatus)
                    changedFields.Add("IsAdmin", $"{originalAdminStatus} -> {newAdminStatus}");
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

        if (string.IsNullOrWhiteSpace(updatedUser.EmailAddress))
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
            UserAccessService.UpdateUser(updatedUser);

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
        return UserAccessService.GetAllUsers().Where(u =>
            u.EmailAddress.ToLower().Contains(emailFilter)).ToList();
    }

    public static List<User> GetUsersByName(string nameFilter)
    {
        nameFilter = nameFilter.ToLower();
        return UserAccessService.GetAllUsers().Where(u =>
            u.FirstName.ToLower().Contains(nameFilter) ||
            u.LastName.ToLower().Contains(nameFilter) ||
            (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(nameFilter)
        ).ToList();
    }

    public static List<User> GetUsersByAdminStatus(bool isAdmin)
    {
        return UserAccessService.GetAllUsers().Where(u => u.IsAdmin == isAdmin).ToList();
    }

    public static User GetUserByEmail(string email)
    {
        return UserAccessService.GetUserInfoByEmail(email);
    }

    public static string GetUserInfo()
    {
        if (!SessionManager.IsLoggedIn())
        {
            errors.Add("User is not logged in.");
            return null;
        }

        return
            $"======================================\n" +
            $"First Name: {SessionManager.CurrentUser.FirstName}\n" +
            $"Last Name: {SessionManager.CurrentUser.LastName}\n" +
            $"Country: {SessionManager.CurrentUser.Country}\n" +
            $"City: {SessionManager.CurrentUser.City}\n" +
            $"Email: {SessionManager.CurrentUser.EmailAddress}\n" +
            $"Phone Number: {SessionManager.CurrentUser.PhoneNumber}\n" +
            $"Birth Date: {SessionManager.CurrentUser.BirthDate.ToString("yyyy-MM-dd")}\n" +
            $"Account Created At: {SessionManager.CurrentUser.AccCreatedAt.ToString("yyyy-MM-dd")}\n" +
            $"======================================\n";
    }
    public static int GetNextUserId()
    {
        int highestId = UserAccessService.GetHighestUserId();
        return highestId + 1;
    }
    
    public static User GetUserByID(int userId)
    {
        return UserAccessService.GetUserById(userId);
    }
}