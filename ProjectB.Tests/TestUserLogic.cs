using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using ProjectB.DataAccess;

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
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!", "Pass123!", "0612345678", "2000-01-01", true, DisplayName = "Valid registration")]
        [DataRow("", "Doe", "USA", "NY", "john@test.com", "Pass123!", "Pass123!", "0612345678", "2000-01-01", false, DisplayName = "Empty first name")]
        [DataRow("John", "", "USA", "NY", "john@test.com", "Pass123!", "Pass123!", "0612345678", "2000-01-01", false, DisplayName = "Empty last name")]
        [DataRow("John", "Doe", "", "NY", "john@test.com", "Pass123!", "Pass123!", "0612345678", "2000-01-01", false, DisplayName = "Empty country")]
        [DataRow("John", "Doe", "USA", "", "john@test.com", "Pass123!", "Pass123!", "0612345678", "2000-01-01", false, DisplayName = "Empty city")]
        [DataRow("John", "Doe", "USA", "NY", "", "Pass123!", "Pass123!", "0612345678", "2000-01-01", false, DisplayName = "Empty email")]
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "", "Pass123!", "0612345678", "2000-01-01", false, DisplayName = "Empty password")]
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!", "", "0612345678", "2000-01-01", false, DisplayName = "Empty confirm password")]
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!", "Pass123!", "", "2000-01-01", false, DisplayName = "Empty phone number")]
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!", "Pass123!", "abc", "2000-01-01", false, DisplayName = "Phone contains letters")]
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!", "Pass123!", "0612345678", "invalid-date", false, DisplayName = "Invalid birth date")]
        [DataRow("John", "Doe", "USA", "NY", "invalid-email", "Pass123!", "Pass123!", "0612345678", "2000-01-01", false, DisplayName = "Invalid email format")]
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!", "DifferentPass!", "0612345678", "2000-01-01", false, DisplayName = "Passwords do not match")]
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
            List<User> insertedUsers = [];

            // Setup mock
            mockUserAccess.Setup(x => x.Insert(It.IsAny<User>()))
                .Callback<User>(u => insertedUsers.Add(u));

            // Act
            var result = UserLogic.Register(firstName, lastName, country, city, emailAddress, password, confirmPassword, phoneNumber, birthDate);

            // Assert
            Assert.AreEqual(expectedResult, result);
            if (expectedResult)
            {
                Assert.AreEqual(1, insertedUsers.Count);
                Assert.AreEqual(emailAddress, insertedUsers[0].Email);
            }
            else
            {
                Assert.AreEqual(0, insertedUsers.Count);
                Assert.IsTrue(UserLogic.errors.Count > 0);
            }
        }

        [DataTestMethod]
        [DataRow("John", "Doe", "USA", "NY", "john@test.com", "Pass123!", "Pass123!", "0612345678", "2000-01-01", true, DisplayName = "Valid registration")]
        [DataRow("Paul", "Atreides", "Arrakis", "Carthag", "paul@atreides.com", "MuadDib123!", "MuadDib123!", "0612345679", "1980-05-01", true, DisplayName = "Valid registration")]
        [DataRow("Duncan", "Idaho", "Caladan", "Arrakeen", "duncan@idaho.com", "Swordmaster1!", "Swordmaster1!", "0612345680", "1975-03-15", true, DisplayName = "Valid registration")]
        public void Register_ValidUser_HasFirstTimeDiscount(
            string firstName,
            string lastName,
            string country,
            string city,
            string emailAddress,
            string password,
            string confirmPassword,
            string phoneNumber,
            string birthDate,
            bool expectedFirstTimeDiscount)
        {
            // Arrange
            List<User> insertedUsers = [];

            // Setup mock
            mockUserAccess.Setup(x => x.Insert(It.IsAny<User>()))
                .Callback<User>(u => insertedUsers.Add(u));

            // Act
            bool result = UserLogic.Register(firstName, lastName, country, city, emailAddress, password, confirmPassword, phoneNumber, birthDate);

            // Assert
            Assert.IsTrue(result, "Registration should succeed for valid input.");
            Assert.AreEqual(1, insertedUsers.Count, "A user should have been inserted.");
            Assert.AreEqual(expectedFirstTimeDiscount, insertedUsers[0].FirstTimeDiscount, "FirstTimeDiscount should be true for new users.");
        }

        [DataTestMethod]
        [DataRow("Alice", "Admin", "USA", "LA", "alice@admin.com", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, true, DisplayName = "Valid admin registration")]
        [DataRow("Alice", "Admin", "USA", "LA", "alice@admin.com", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Customer, true, DisplayName = "Valid customer registration")]
        [DataRow("", "Admin", "USA", "LA", "alice@admin.com", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Empty first name")]
        [DataRow("Alice", "", "USA", "LA", "alice@admin.com", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Empty last name")]
        [DataRow("Alice", "Admin", "", "LA", "alice@admin.com", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Empty country")]
        [DataRow("Alice", "Admin", "USA", "", "alice@admin.com", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Empty city")]
        [DataRow("Alice", "Admin", "USA", "LA", "", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Empty email")]
        [DataRow("Alice", "Admin", "USA", "LA", "alice@admin.com", "", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Empty password")]
        [DataRow("Alice", "Admin", "USA", "LA", "alice@admin.com", "AdminPass1!", "06abc45678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Phone contains letters")]
        [DataRow("Alice", "Admin", "USA", "LA", "alice@admin.com", "AdminPass1!", "", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Empty phone number")]
        [DataRow("Alice", "Admin", "USA", "LA", "aliceadmin.com", "AdminPass1!", "0612345678", "1980-05-05", "2024-01-01", UserRole.Admin, false, DisplayName = "Invalid email format")]
        [DataRow("Alice", "Admin", "USA", "LA", "alice@admin.com", "AdminPass1!", "0612345678", "not-a-date", "2024-01-01", UserRole.Admin, false, DisplayName = "Invalid birth date")]
        public void AdminRegister_ValidatesInputsCorrectly(
            string firstName,
            string lastName,
            string country,
            string city,
            string emailAddress,
            string password,
            string phoneNumber,
            string birthDate,
            string accCreatedAt,
            UserRole role,
            bool expectedResult)
        {
            // Arrange
            List<User> insertedUsers = [];

            // Setup mock
            mockUserAccess.Setup(x => x.Insert(It.IsAny<User>()))
                .Callback<User>(insertedUsers.Add);
            DateTime accCreatedAtDate = DateTime.Parse(accCreatedAt);

            // Act
            bool result = UserLogic.Register(
                firstName, lastName, country, city, emailAddress, password, phoneNumber, birthDate, accCreatedAtDate, role);

            // Assert
            Assert.AreEqual(expectedResult, result);
            if (expectedResult)
            {
                Assert.AreEqual(1, insertedUsers.Count);
                Assert.AreEqual(emailAddress, insertedUsers[0].Email);
            }
            else
            {
                Assert.AreEqual(0, insertedUsers.Count);
                Assert.IsTrue(UserLogic.errors.Count > 0);
            }
        }

        [DataTestMethod]
        [DataRow("john@test.com", "Pass123!", true, DisplayName = "Correct credentials")]
        [DataRow("john@test.com", "WrongPass", false, DisplayName = "Incorrect password")]
        [DataRow("unknown@test.com", "Pass123!", false, DisplayName = "Unknown email")]
        [DataRow("", "Pass123!", false, DisplayName = "Empty email")]
        [DataRow("john@test.com", "", false, DisplayName = "Empty password")]
        public void Login_ValidatesCredentialsCorrectly(string email, string password, bool expectedResult)
        {
            // Arrange
            var users = new Dictionary<string, string>
            {
                { "john@test.com", "Pass123!" },
                { "alice@admin.com", "AdminPass1!" }
            };

            // Setup mock
            mockUserAccess.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string e, string p) =>
                {
                    if (users.TryGetValue(e, out var correctPassword) && correctPassword == p)
                        return new User { Email = e, Password = p };
                    return null;
                });

            // Act
            var result = UserLogic.Login(email, password);

            // Assert
            Assert.AreEqual(expectedResult, result);
            if (!expectedResult)
                Assert.IsTrue(UserLogic.errors.Count > 0);
        }

        [DataTestMethod]
        [DataRow("john@test.com", false, DisplayName = "Duplicate email")]
        [DataRow("alice@admin.com", false, DisplayName = "Another duplicate email")]
        [DataRow("notfound@test.com", true, DisplayName = "Non-existing email")]
        [DataRow("invalidemail", false, DisplayName = "No @ or . in email")]
        [DataRow("missingat.com", false, DisplayName = "Missing @")]
        [DataRow("missingdot@testcom", false, DisplayName = "Missing .")]
        [DataRow("", false, DisplayName = "Empty email")]
        public void RegisterCheckEmail_ValidatesEmailCorrectly(string email, bool expectedResult)
        {
            // Arrange
            var existingEmails = new List<string> { "john@test.com", "alice@admin.com" };

            // Setup mock
            mockUserAccess.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>()))
                .Returns((string e) => existingEmails.Contains(e) ? new User { Email = e } : null);

            // Act
            bool result = UserLogic.Register(
                "First", "Last", "Country", "City", email, "Password1!", "Password1!", "0612345678", "2000-01-01"
            );

            // Assert
            Assert.AreEqual(expectedResult, result);
            if (!expectedResult)
                Assert.IsTrue(UserLogic.errors.Any(msg => msg.Contains("Email validation failed")));
        }

        [DataTestMethod]
        [DataRow("john@test.com", "Pass123!", true, DisplayName = "Correct password")]
        [DataRow("john@test.com", "WrongPass", false, DisplayName = "Incorrect password")]
        [DataRow("unknown@test.com", "Pass123!", false, DisplayName = "User not found")]
        [DataRow("", "Pass123!", false, DisplayName = "Empty email")]
        [DataRow("john@test.com", "", false, DisplayName = "Empty password")]
        public void VerifyPassword_ValidatesCredentialsCorrectly(string email, string password, bool expectedResult)
        {
            // Arrange
            var users = new Dictionary<string, string>
            {
                { "john@test.com", "Pass123!" },
                { "alice@admin.com", "AdminPass1!" }
            };

            // Setup mock
            mockUserAccess.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>()))
                .Returns((string e) =>
                {
                    if (users.TryGetValue(e, out var correctPassword))
                        return new User { Email = e, Password = correctPassword };
                    return null;
                });

            UserLogic.errors.Clear();

            // Act
            bool result = UserLogic.VerifyPassword(email, password);

            // Assert
            Assert.AreEqual(expectedResult, result);
            if (!expectedResult)
                Assert.IsTrue(UserLogic.errors.Count > 0);
        }

        [TestMethod]
        public void GetAllUsers_ReturnsAllUsers()
        {
            // Arrange
            var mockUsers = new List<User>
            {
                new User { Email = "john@test.com" },
                new User { Email = "alice@admin.com" }
            };
            // Setup mock
            mockUserAccess.Setup(x => x.GetAll()).Returns(mockUsers);

            // Act
            var result = UserLogic.GetAllUsers();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("john@test.com", result[0].Email);
            Assert.AreEqual("alice@admin.com", result[1].Email);
        }

        [DataTestMethod]
        [DataRow("Jane", "Smith", "jane@guest.com", "0612345678", "1990-01-01", DisplayName = "Valid guest update")]
        [DataRow("John", "Doe", "john@guest.com", "0611111111", "1985-12-31", DisplayName = "Another valid guest update")]
        [DataRow("Emily", "Brown", "emily@guest.com", "0699999999", "2002-07-15", DisplayName = "Yet another valid guest update")]
        public void UpdateGuestUser_UpdatesGuestCorrectly(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            string birthDate)
        {
            // Arrange
            SessionManager.Logout();
            Assert.IsNull(SessionManager.CurrentUser);

            SessionManager.SetGuestUser();
            UserLogic.errors.Clear();

            // Act
            UserLogic.UpdateGuestUser(firstName, lastName, email, phoneNumber, birthDate);

            // Assert
            User? updatedGuestUser = SessionManager.CurrentUser;
            Assert.IsNotNull(updatedGuestUser);
            Assert.AreEqual(firstName, updatedGuestUser.FirstName);
            Assert.AreEqual(lastName, updatedGuestUser.LastName);
            Assert.AreEqual(email, updatedGuestUser.Email);
            Assert.AreEqual(phoneNumber, updatedGuestUser.PhoneNumber);
            Assert.IsFalse(updatedGuestUser.FirstTimeDiscount);
            Assert.AreEqual(DateTime.Parse(birthDate), updatedGuestUser.BirthDate);
            Assert.AreEqual(UserRole.Guest, updatedGuestUser.Role);
            Assert.AreEqual(0, UserLogic.errors.Count);
        }

        [DataTestMethod]
        [DataRow(1, "John", "Doe", "USA", "NY", true, DisplayName = "Valid update for existing user")]
        [DataRow(1, "", "Doe", "USA", "NY", false, DisplayName = "Empty first name")]
        [DataRow(1, "John", "", "USA", "NY", false, DisplayName = "Empty last name")]
        [DataRow(999, "John", "Doe", "USA", "NY", true, DisplayName = "Valid update for non-existing user (should add)")]
        public void UpdateUser_ValidatesAndUpdatesCorrectly(
    int userId,
    string newFirstName,
    string newLastName,
    string newCountry,
    string newCity,
    bool expectedResult)
        {
            // Arrange
            User user = new User
            {
                UserID = userId,
                FirstName = newFirstName,
                LastName = newLastName,
                Country = newCountry,
                City = newCity,
                Email = "test@test.com",
                Password = "Pass123!",
                PhoneNumber = "1234567890",
                BirthDate = DateTime.Parse("2000-01-01"),
                Role = UserRole.Customer
            };

            List<User> existingUsers = new List<User>();
            if (userId == 1)
            {
                existingUsers.Add(new User
                {
                    UserID = 1,
                    FirstName = "Original",
                    LastName = "User",
                    Country = "USA",
                    City = "NY",
                    Email = "test@test.com",
                    Password = "Pass123!",
                    PhoneNumber = "1234567890",
                    BirthDate = DateTime.Parse("2000-01-01"),
                    Role = UserRole.Customer
                });
            }
            List<User> updatedUsers = [];
            SessionManager.SetCurrentUser(new User
            {
                UserID = userId,
                FirstName = "Original",
                LastName = "User",
                Country = "USA",
                City = "NY",
                Email = "test@test.com",
                Password = "Pass123!",
                PhoneNumber = "1234567890",
                BirthDate = DateTime.Parse("2000-01-01"),
                Role = UserRole.Customer
            });
            UserLogic.errors.Clear();

            // Setup mock
            mockUserAccess.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns((int id) => existingUsers.FirstOrDefault(u => u.UserID == id));
            mockUserAccess.Setup(x => x.Update(It.IsAny<User>()))
                .Callback<User>(u => updatedUsers.Add(u));
            UserLogic.UserAccessService = mockUserAccess.Object;

            // Act
            bool result = UserLogic.UpdateUser(user);

            // Assert
            Assert.AreEqual(expectedResult, result);

            if (expectedResult)
            {
                // Should have called Update and added to updatedUsers
                Assert.AreEqual(1, updatedUsers.Count);
                Assert.AreEqual(user.UserID, updatedUsers[0].UserID);

                // Should update SessionManager.CurrentUser if IDs match
                Assert.AreEqual(user.UserID, SessionManager.CurrentUser.UserID);
                Assert.AreEqual(user.FirstName, SessionManager.CurrentUser.FirstName);
                Assert.AreEqual(user.LastName, SessionManager.CurrentUser.LastName);
                Assert.AreEqual(user.City, SessionManager.CurrentUser.City);
                Assert.AreEqual(user.Country, SessionManager.CurrentUser.Country);
            }
            else
            {
                Assert.AreEqual(0, updatedUsers.Count);
                Assert.IsTrue(UserLogic.errors.Count > 0);
            }
        }

        [DataTestMethod]
        [DataRow("test", 2, DisplayName = "Two users with 'test' in email")]
        [DataRow("john.doe", 1, DisplayName = "One user with 'john.doe' in email")]
        [DataRow("nonexistent", 0, DisplayName = "No users with 'nonexistent' in email")]
        [DataRow("example", 2, DisplayName = "Two users with 'example' in email")]
        [DataRow("@test.com", 2, DisplayName = "Two users with '@test.com' in email")]
        public void GetUsersByEmail_IfExists_ReturnsCorrectUsers(string emailFilter, int expectedCount)
        {
            // Arrange
            List<User> mockUsers = new List<User>
            {
                new User { Email = "test@test.com" },
                new User { Email = "test2@test.com" },
                new User { Email = "john.doe@example.com" },
                new User { Email = "other@example.com" }
            };

            // Setup mock
            mockUserAccess.Setup(x => x.GetAll()).Returns(mockUsers);

            // Act
            List<User> results = UserLogic.GetUsersByEmail(emailFilter);

            // Assert
            Assert.AreEqual(expectedCount, results.Count);
        }

        [DataTestMethod]
        [DataRow("Atreides", 2, DisplayName = "Two users with 'Atreides' in name")]
        [DataRow("Harkonnen", 1, DisplayName = "One user with 'Harkonnen' in name")]
        [DataRow("Valya", 1, DisplayName = "One user with 'Valya' in name")]
        [DataRow("Nonexistent", 0, DisplayName = "No users with 'Nonexistent' in name")]
        [DataRow("Duncan Idaho", 1, DisplayName = "One user with 'Duncan Idaho' in name")]
        public void GetUsersByName_IfExists_ReturnsCorrectUsers(string nameFilter, int expectedCount)
        {
            // Arrange
            List<User> mockUsers = new List<User>
            {
                new User { FirstName = "Paul", LastName = "Atreides" },
                new User { FirstName = "Leto", LastName = "Atreides" },
                new User { FirstName = "Valya", LastName = "Harkonnen" },
                new User { FirstName = "Duncan", LastName = "Idaho" }
            };

            // Setup mock
            mockUserAccess.Setup(x => x.GetAll()).Returns(mockUsers);

            // Act
            List<User> results = UserLogic.GetUsersByName(nameFilter);

            // Assert
            Assert.AreEqual(expectedCount, results.Count);
        }

        [DataTestMethod]
        [DataRow(UserRole.Admin, 2, DisplayName = "Two Admin users")]
        [DataRow(UserRole.Customer, 3, DisplayName = "Two Customer users")]
        public void GetUsersByRole_ReturnsAllUsersByRole(UserRole role, int expectedCount)
        {
            // Arrange
            List<User> mockUsers = new List<User>
            {
                new User { Role = UserRole.Admin },
                new User { Role = UserRole.Admin },
                new User { Role = UserRole.Customer },
                new User { Role = UserRole.Customer },
                new User { Role = UserRole.Customer }
            };

            // Setup mock
            mockUserAccess.Setup(x => x.GetAll()).Returns(mockUsers);

            // Act
            List<User> results = UserLogic.GetUsersByRole(role);

            // Assert
            Assert.AreEqual(expectedCount, results.Count);
        }

        [DataTestMethod]
        [DataRow("paul@atreides.com", true, DisplayName = "User exists: paul@atreides.com")]
        [DataRow("duncan@idaho.com", true, DisplayName = "User exists: duncan@idaho.com")]
        [DataRow("unknown@airtreides.com", false, DisplayName = "User does not exist")]
        [DataRow("jessica@harkonnen.com", true, DisplayName = "User exists: jessica@harkonnen.com")]
        [DataRow("baron@harkonnen.com", true, DisplayName = "User exists: baron@harkonnen.com")]
        public void GetUserByEmail_IfExists_ReturnsCorrectUsers(string email, bool expectedExists)
        {
            // Arrange
            List<User> mockUsers = new List<User>
            {
                new User { Email = "paul@atreides.com" },
                new User { Email = "duncan@idaho.com" },
                new User { Email = "jessica@harkonnen.com" },
                new User { Email = "baron@harkonnen.com" }
            };

            // Setup mock
            mockUserAccess.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>()))
                .Returns((string e) => mockUsers.FirstOrDefault(u => u.Email == e));


            // Act
            User? result = UserLogic.GetUserByEmail(email);

            // Assert
            if (expectedExists)
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(email, result.Email);
            }
            else
            {
                Assert.IsNull(result);
            }
        }

        [DataTestMethod]
        [DataRow(1, true, DisplayName = "User with ID 1 exists")]
        [DataRow(2, true, DisplayName = "User with ID 2 exists")]
        [DataRow(999, false, DisplayName = "User with ID 999 does not exist")]
        public void GetUserById_IfExists_ReturnsCorrectUser(int userId, bool expectedExists)
        {
            // Arrange
            List<User> mockUsers = new List<User>
            {
                new User { UserID = 1, FirstName = "Paul", LastName = "Atreides" },
                new User { UserID = 2, FirstName = "Duncan", LastName = "Idaho" }
            };

            // Setup mock
            mockUserAccess.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns((int id) => mockUsers.FirstOrDefault(u => u.UserID == id));

            // Act
            User? result = UserLogic.GetUserByID(userId);

            // Assert
            if (expectedExists)
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(userId, result.UserID);
            }
            else
            {
                Assert.IsNull(result);
            }
        }
    }
}