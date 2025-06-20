using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TestUserLogic
    {
        private Mock<IUserAccess> mockUserAccess;

        [TestInitialize]
        public void Setup()
        {
            UserLogic.errors.Clear();
            mockUserAccess = new Mock<IUserAccess>();
            UserLogic.UserAccessService = mockUserAccess.Object;
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
            // Arrange
            mockUserAccess.Setup(x => x.GetAllUsers()).Returns(new List<User>());

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
        [DataRow(1, "John", "Doe", "USA", "NY", true)]
        [DataRow(1, "", "Doe", "USA", "NY", false)]
        [DataRow(1, "John", "", "USA", "NY", false)]
        [DataRow(999, "John", "Doe", "USA", "NY", true)]
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

            var existingUsers = new List<User>();
            if (userId == 1)
            {
                existingUsers.Add(new User { UserID = 1, FirstName = "Original", LastName = "User" });
            }
            mockUserAccess.Setup(x => x.GetAllUsers()).Returns(existingUsers);

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
            // Arrange
            var mockUsers = new List<User>
            {
                new User { EmailAddress = "test@test.com" },
                new User { EmailAddress = "test2@test.com" },
                new User { EmailAddress = "john.doe@example.com" },
                new User { EmailAddress = "other@example.com" }
            };
            mockUserAccess.Setup(x => x.GetAllUsers()).Returns(mockUsers);

            // Act
            var results = UserLogic.GetUsersByEmail(emailFilter);

            // Assert
            Assert.AreEqual(expectedCount, results.Count);
        }

        [DataTestMethod]
        [DataRow(true, 2)]
        [DataRow(false, 3)]
        public void GetUsersByAdminStatus_ReturnsCorrectUsers(bool isAdmin, int expectedCount)
        {
            // Arrange
            var mockUsers = new List<User>
            {
                new User { IsAdmin = true },
                new User { IsAdmin = true },
                new User { IsAdmin = false },
                new User { IsAdmin = false },
                new User { IsAdmin = false }
            };
            mockUserAccess.Setup(x => x.GetAllUsers()).Returns(mockUsers);

            // Act
            var results = UserLogic.GetUsersByAdminStatus(isAdmin);

            // Assert
            Assert.AreEqual(expectedCount, results.Count);
        }
    }
}