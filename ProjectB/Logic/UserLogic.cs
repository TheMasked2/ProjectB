using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


//This class is not static so later on we can use inheritance and interfaces
public static class UserLogic
{
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
            userID: UserAccess.GetHighestUserId() + 1,
            firstName, lastName, country, city, emailAddress, password, 
            phoneNumber.ToString(), birthDate, DateTime.Now, isAdmin: false, firstTimeDiscount: true
        );
    


        UserAccess.AddUser(user);
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
            userID: UserAccess.GetHighestUserId() + 1,
            firstName, lastName, country, city, emailAddress, password,
            phoneNumber.ToString(), birthDate, accCreatedAt, isAdmin
        );

        UserAccess.AddUser(user);

        Logger.LogUserCreation(SessionManager.CurrentUser, user);

        return true;
    }

    public static bool Login(string email, string password)
    {
        if (SessionManager.IsLoggedIn())
        {
            errors.Add("You are already logged in.");
            return false;
        }

        User loggedInUser = UserAccess.Login(email, password);

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
        if (UserAccess.GetUserInfoByEmail(email) != null)
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
        var userInfo = UserAccess.GetUserInfoByEmail(email);

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
        return UserAccess.GetAllUsers();
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
            originalUser = UserAccess.GetUserInfoByID(updatedUser.UserID);
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
            UserAccess.UpdateUser(updatedUser);
            
            // Log if admin update and if any fields actually changed
            if (isAdminUpdate && originalUser != null && changedFields != null && changedFields.Count > 0)
            {
                Logger.LogUserEdit(SessionManager.CurrentUser, updatedUser, changedFields);
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
        return UserAccess.GetAllUsers().Where(u => 
            u.EmailAddress.ToLower().Contains(emailFilter)).ToList();
    }
    
    public static List<User> GetUsersByName(string nameFilter)
    {
        nameFilter = nameFilter.ToLower();
        return UserAccess.GetAllUsers().Where(u => 
            u.FirstName.ToLower().Contains(nameFilter) || 
            u.LastName.ToLower().Contains(nameFilter) ||
            (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(nameFilter)
        ).ToList();
    }
    
    public static List<User> GetUsersByAdminStatus(bool isAdmin)
    {
        return UserAccess.GetAllUsers().Where(u => u.IsAdmin == isAdmin).ToList();
    }
    
    public static User GetUserByEmail(string email)
    {
        return UserAccess.GetUserInfoByEmail(email);
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
        int highestId = UserAccess.GetHighestUserId();
        return highestId + 1;
    }
}