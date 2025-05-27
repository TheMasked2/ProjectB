using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class LoggerUnitTests
    {
        /// <summary>
        /// Tests if Logger.LogUserCreation calls WriteLogEntry and returns the expected result
        /// (true or false) based on the input data and the mock's return value.
        /// </summary>
        [DataTestMethod]
        [DataRow(0, "", "", 0, "", "", "", false, false)] // All fields empty or zero
        [DataRow(1, "Admin", "User", 2, null, null, null, false, true)] // Created user fields null
        [DataRow(1, null, null, 2, "Test", "User", "test@example.com", false, true)] // Admin user fields null
        [DataRow(1, "Admin", "User", 2, "Test", "User", "test@example.com", false, true)] // Normal valid case
        public void LogUserCreation_CallsWriteLogEntry_ReturnsExpected(
            int adminId, string adminFirstName, string adminLastName,
            int createdId, string createdFirstName, string createdLastName,
            string createdEmail, bool createdIsAdmin, bool expectedResult)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>())).Returns(expectedResult);
            Logger.LoggerAccessService = mockLoggerAccess.Object;

            var adminUser = new User { UserID = adminId, FirstName = adminFirstName, LastName = adminLastName };
            var createdUser = new User { UserID = createdId, FirstName = createdFirstName, LastName = createdLastName, EmailAddress = createdEmail, IsAdmin = createdIsAdmin };

            // Act
            var result = Logger.LogUserCreation(adminUser, createdUser);

            // Assert
            Assert.AreEqual(expectedResult, result);
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests if Logger.LogUserEdit calls WriteLogEntry and returns the expected result
        /// (true or false) for various edit scenarios.
        /// </summary>
        [DataTestMethod]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "LastName:User->Smith|City:OldTown->NewTown", true)]
        [DataRow(5, "Super", "Visor", 6, "Another", "Editor", "PhoneNumber:12345->67890|IsAdmin:False->True", false)]
        [DataRow(2, "Jane", "Doe", 10, "John", "Public", "FirstName:John->Jonathan|Country:USA->Canada|PhoneNumber:5550100->5550199", true)]
        [DataRow(1, "Root", "User", 1, "Root", "User", "IsAdmin:True->False", true)]
        [DataRow(7, "Manager", "Alpha", 15, "Employee", "Beta", "LastName:Beta->Gamma|City:Metro->Suburbia|Country:UK->France", true)]
        [DataRow(9, "Temp", "Admin", 25, "Contractor", "Gamma", "FirstName:Contractor->Consultant|PhoneNumber:N/A->111222333", false)]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "", true)] // Empty changedFieldsString
        [DataRow(1, "Admin", "User", 2, "Test", "User", null, false)] // Null changedFieldsString
        [DataRow(1, "Admin", "User", 2, "Test", "User", "FirstName:Test->Tester", true)] // Normal case
        public void LogUserEdit_CallsWriteLogEntry_ReturnsExpected(
            int adminId, string adminFirstName, string adminLastName,
            int editedId, string editedFirstName, string editedLastName,
            string changedFieldsString, bool expectedResult)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>())).Returns(expectedResult);
            Logger.LoggerAccessService = mockLoggerAccess.Object;

            var adminUser = new User { UserID = adminId, FirstName = adminFirstName, LastName = adminLastName };
            var editedUser = new User { UserID = editedId, FirstName = editedFirstName, LastName = editedLastName };

            var changedFields = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(changedFieldsString))
            {
                var pairs = changedFieldsString.Split('|');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split(new[] { ':' }, 2);
                    if (keyValue.Length == 2)
                    {
                        changedFields[keyValue[0]] = keyValue[1];
                    }
                }
            }

            // Act
            var result = Logger.LogUserEdit(adminUser, editedUser, changedFields);

            // Assert
            Assert.AreEqual(expectedResult, result);
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that Logger.LogUserCreation writes a log entry with the correct format and content
        /// (excluding the timestamp), matching the CSV logbook structure.
        /// </summary>
        [DataTestMethod]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "test@example.com", false, true,
            "CREATE_USER;1;Admin User;2;Test User;Email=test@example.com|Admin=False")]
        [DataRow(2, "Root", "Admin", 3, "Alpha", "Beta", "alpha.beta@mail.com", true, false,
            "CREATE_USER;2;Root Admin;3;Alpha Beta;Email=alpha.beta@mail.com|Admin=True")]
        [DataRow(0, "", "", 0, "", "", "", false, false,
            "CREATE_USER;0; ;0; ;Email=|Admin=False")]
        public void LogUserCreation_WritesCorrectLogEntryFormat(
            int adminId, string adminFirstName, string adminLastName,
            int createdId, string createdFirstName, string createdLastName,
            string createdEmail, bool createdIsAdmin, bool expectedResult,
            string expectedLogEntry)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            string capturedLogEntry = null;
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>()))
                .Callback<string>(entry => capturedLogEntry = entry)
                .Returns(expectedResult);
            Logger.LoggerAccessService = mockLoggerAccess.Object;

            var adminUser = new User { UserID = adminId, FirstName = adminFirstName, LastName = adminLastName };
            var createdUser = new User { UserID = createdId, FirstName = createdFirstName, LastName = createdLastName, EmailAddress = createdEmail, IsAdmin = createdIsAdmin };

            // Act
            var result = Logger.LogUserCreation(adminUser, createdUser);

            // Assert
            Assert.AreEqual(expectedResult, result);
            Assert.IsNotNull(capturedLogEntry);

            // Remove timestamp for comparison
            var logParts = capturedLogEntry.Split(';');
            Assert.AreEqual(7, logParts.Length, "Log entry should have 7 semicolon-separated columns.");
            var logEntryWithoutTimestamp = string.Join(";", logParts.Skip(1));

            // Check the log entry matches expected format/content (excluding timestamp)
            Assert.AreEqual(expectedLogEntry, logEntryWithoutTimestamp);
        }
    }
}