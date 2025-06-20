using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

[TestClass]
public class TestUserLogic
{
    [TestInitialize]
    public void Setup()
    {
        UserLogic.errors.Clear();
    }

    [DataTestMethod]
    [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!","Pass123!", "1234567890", "2000-01-01", true)]
    [DataRow("", "Doe", "USA", "NY", "john@test.com", "Pass123!","Pass123!", "1234567890", "2000-01-01", false)]
    [DataRow("John", "", "USA", "NY", "john@test.com", "Pass123!","Pass123!", "1234567890", "2000-01-01", false)]
    [DataRow("John", "Doe", "", "NY", "john@test.com", "Pass123!","Pass123!", "1234567890", "2000-01-01", false)]
    [DataRow("John", "Doe", "USA", "", "john@test.com", "Pass123!","Pass123!", "1234567890", "2000-01-01", false)]
    [DataRow("John", "Doe", "USA", "NY", "", "Pass123!", "1234567890","1234567890", "2000-01-01", false)]
    [DataRow("John", "Doe", "USA", "NY", "invalid-email", "Pass123!","Pass123!", "1234567890", "2000-01-01", false)]
    [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!","Pass123!", "abc", "2000-01-01", false)]
    [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!","Pass123!", "1234567890", "invalid-date", false)]
    public void Register_ValidatesInputsCorrectly(
        string firstName,
        string lastName,
        string country,
        string city,
        string emailAddress,
        string password,
        string confirmPassword,
        string phoneNumber,
        string birthDate,
        bool expectedResult)
    {
        // Act
        var result = UserLogic.Register(firstName, lastName, country, city, emailAddress, password, confirmPassword, phoneNumber, birthDate);

        // Assert
        Assert.AreEqual(expectedResult, result);
        if (!expectedResult)
        {
            Assert.IsTrue(UserLogic.errors.Count > 0);
        }
    }

    [DataTestMethod]
    [DataRow("test@test.com", "Pass123!", true)]
    [DataRow("", "Pass123!", false)]
    [DataRow("test@test.com", "", false)]
    [DataRow("wrong@test.com", "Pass123!", false)]
    [DataRow("test@test.com", "WrongPass!", false)]
    public void Login_ValidatesCredentialsCorrectly(string email, string password, bool expectedResult)
    {
        // Act
        var result = UserLogic.Login(email, password);

        // Assert
        Assert.AreEqual(expectedResult, result);
        if (!expectedResult)
        {
            Assert.IsTrue(UserLogic.errors.Count > 0);
        }
    }

    [DataTestMethod]
    [DataRow(1, "John", "Doe", "USA", "NY", true)]
    [DataRow(1, "", "Doe", "USA", "NY", false)]
    [DataRow(1, "John", "", "USA", "NY", false)]
    [DataRow(999, "John", "Doe", "USA", "NY", false)]
    public void UpdateUser_ValidatesAndUpdatesCorrectly(
        int userId,
        string newFirstName,
        string newLastName,
        string newCountry,
        string newCity,
        bool expectedResult)
    {
        // Arrange
        var user = new User
        {
            UserID = userId,
            FirstName = newFirstName,
            LastName = newLastName,
            Country = newCountry,
            City = newCity,
            EmailAddress = "test@test.com",
            Password = "Pass123!",
            PhoneNumber = "1234567890",
            BirthDate = DateTime.Parse("2000-01-01")
        };

        // Act
        var result = UserLogic.UpdateUser(user);

        // Assert
        Assert.AreEqual(expectedResult, result);
        if (!expectedResult)
        {
            Assert.IsTrue(UserLogic.errors.Count > 0);
        }
    }

    [DataTestMethod]
    [DataRow("test", 2)]
    [DataRow("john.doe", 1)]
    [DataRow("nonexistent", 0)]
    public void GetUsersByEmail_ReturnsCorrectUsers(string emailFilter, int expectedCount)
    {
        // Act
        var results = UserLogic.GetUsersByEmail(emailFilter);

        // Assert
        Assert.AreEqual(expectedCount, results.Count);
    }

    [DataTestMethod]
    [DataRow("John", 2)]
    [DataRow("Doe", 1)]
    [DataRow("John Doe", 1)]
    [DataRow("nonexistent", 0)]
    public void GetUsersByName_ReturnsCorrectUsers(string nameFilter, int expectedCount)
    {
        // Act
        var results = UserLogic.GetUsersByName(nameFilter);

        // Assert
        Assert.AreEqual(expectedCount, results.Count);
    }

    [DataTestMethod]
    [DataRow(true, 2)]
    [DataRow(false, 3)]
    public void GetUsersByAdminStatus_ReturnsCorrectUsers(bool isAdmin, int expectedCount)
    {
        // Act
        var results = UserLogic.GetUsersByAdminStatus(isAdmin);

        // Assert
        Assert.AreEqual(expectedCount, results.Count);
    }
}