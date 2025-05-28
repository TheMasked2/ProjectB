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
        /// Tests that LogUserCreation returns true when the underlying WriteLogEntry succeeds.
        /// </summary>
        [DataTestMethod]
        [DataRow(1, "Admin", "User", 2, null, null, null, false, DisplayName = "Created user null fields, success")]
        [DataRow(1, null, null, 2, "Test", "User", "test@example.com", false, DisplayName = "Admin user null fields, success")]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "test@example.com", false, DisplayName = "Valid case, success")]
        public void LogUserCreation_WhenWriteSucceeds_ReturnsTrue(
            int adminId, string adminFirstName, string adminLastName,
            int createdId, string createdFirstName, string createdLastName,
            string createdEmail, bool createdIsAdmin)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>())).Returns(true);
            Logger.LoggerAccessService = mockLoggerAccess.Object;

            var adminUser = new User { UserID = adminId, FirstName = adminFirstName, LastName = adminLastName };
            var createdUser = new User { UserID = createdId, FirstName = createdFirstName, LastName = createdLastName, EmailAddress = createdEmail, IsAdmin = createdIsAdmin };

            // Act
            var result = Logger.LogUserCreation(adminUser, createdUser);

            // Assert
            Assert.IsTrue(result);
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that LogUserCreation returns false when the underlying WriteLogEntry fails.
        /// </summary>
        [DataTestMethod]
        [DataRow(0, "", "", 0, "", "", "", false, DisplayName = "All fields empty/zero, fail")]
        public void LogUserCreation_WhenWriteFails_ReturnsFalse(
            int adminId, string adminFirstName, string adminLastName,
            int createdId, string createdFirstName, string createdLastName,
            string createdEmail, bool createdIsAdmin)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>())).Returns(false);
            Logger.LoggerAccessService = mockLoggerAccess.Object;

            var adminUser = new User { UserID = adminId, FirstName = adminFirstName, LastName = adminLastName };
            var createdUser = new User { UserID = createdId, FirstName = createdFirstName, LastName = createdLastName, EmailAddress = createdEmail, IsAdmin = createdIsAdmin };

            // Act
            var result = Logger.LogUserCreation(adminUser, createdUser);

            // Assert
            Assert.IsFalse(result);
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that LogUserEdit returns true when the underlying WriteLogEntry succeeds.
        /// </summary>
        [DataTestMethod]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "LastName:User->Smith|City:OldTown->NewTown", DisplayName = "Multiple changes, success")]
        [DataRow(1, "Root", "User", 1, "Root", "User", "IsAdmin:True->False", DisplayName = "Single change (IsAdmin), success")]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "", DisplayName = "Empty changes string, success")]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "FirstName:Test->Tester", DisplayName = "Single change, success")]
        public void LogUserEdit_WhenWriteSucceeds_ReturnsTrue(
            int adminId, string adminFirstName, string adminLastName,
            int editedId, string editedFirstName, string editedLastName,
            string changedFieldsString)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>())).Returns(true);
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
            Assert.IsTrue(result);
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that LogUserEdit returns false when the underlying WriteLogEntry fails.
        /// </summary>
        [DataTestMethod]
        [DataRow(1, "Admin", "User", 2, "Test", "User", null, DisplayName = "Null changes string, mock false")]
        [DataRow(3, "Supervisor", "Main", 4, "Edited", "Person", "Department:Sales->Marketing|Title:Rep->Manager", DisplayName = "Multiple valid changes, mock false")]
        [DataRow(5, "Another", "Admin", 6, "User", "ToEdit", "", DisplayName = "Empty changes string, mock false")]
        public void LogUserEdit_WhenWriteFails_ReturnsFalse(
            int adminId, string adminFirstName, string adminLastName,
            int editedId, string editedFirstName, string editedLastName,
            string changedFieldsString)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>())).Returns(false);
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
            Assert.IsFalse(result);
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that Logger.LogUserCreation writes a log entry with the correct format and content
        /// (excluding the timestamp), matching the CSV logbook structure.
        /// </summary>
        [DataTestMethod]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "test@example.com", false, true, "CREATE_USER;1;Admin User;2;Test User;Email=test@example.com|Admin=False", DisplayName = "Valid case, true")]
        [DataRow(2, "Root", "Admin", 3, "Alpha", "Beta", "alpha.beta@mail.com", true, false, "CREATE_USER;2;Root Admin;3;Alpha Beta;Email=alpha.beta@mail.com|Admin=True", DisplayName = "Admin created, mock false")]
        [DataRow(0, "", "", 0, "", "", "", false, false, "CREATE_USER;0; ;0; ;Email=|Admin=False", DisplayName = "Empty/zero IDs and names, false")]
        public void LogUserCreation_WritesCorrectLogEntryFormat(
            int adminId, string adminFirstName, string adminLastName,
            int createdId, string createdFirstName, string createdLastName,
            string createdEmail, bool createdIsAdmin, bool expectedResult,
            string expectedLogEntryPartial)
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
            
            if (expectedResult)
            {
                 Assert.IsNotNull(capturedLogEntry, "Log entry should have been captured when write operation was expected to succeed.");
                 if (capturedLogEntry != null)
                 {
                    var logParts = capturedLogEntry.Split(';');
                    Assert.IsTrue(logParts.Length >= 6, $"Log entry should have at least 6 semicolon-separated columns for content check, but found {logParts.Length}. Entry: {capturedLogEntry}");
                    var logEntryWithoutTimestamp = string.Join(";", logParts.Skip(1));
                    Assert.AreEqual(expectedLogEntryPartial, logEntryWithoutTimestamp);
                 }
            }
            else
            {
                Assert.IsNull(capturedLogEntry, "Log entry should not have been captured when write operation was expected to fail or not occur.");
            }
            
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
        }
    }
}