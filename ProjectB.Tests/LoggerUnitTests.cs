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
        [DataTestMethod]
        [DataRow(1, "Admin", "User", UserRole.Admin, 2, "Test", "User", "test@example.com", UserRole.Customer, DisplayName = "Valid case")]
        [DataRow(1, null, null, UserRole.Admin, 2, "Test", "User", "test@example.com", UserRole.Customer, DisplayName = "Admin user null fields")]
        [DataRow(1, "Admin", "User", UserRole.Admin, 2, null, null, null, UserRole.Customer, DisplayName = "Created user null fields")]
        public void LogUserCreation_CallsWriteLogEntryWithCorrectFormat(
            int adminId, string adminFirstName, string adminLastName, UserRole adminRole,
            int createdId, string createdFirstName, string createdLastName, string createdEmail, UserRole createdRole)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            string capturedLogEntry = null;
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>()))
                .Callback<string>(entry => capturedLogEntry = entry);
            LoggerLogic.LoggerAccessService = mockLoggerAccess.Object;

            var adminUser = new User { UserID = adminId, FirstName = adminFirstName, LastName = adminLastName, Role = adminRole };
            var createdUser = new User { UserID = createdId, FirstName = createdFirstName, LastName = createdLastName, Email = createdEmail, Role = createdRole };

            // Act
            LoggerLogic.LogUserCreation(adminUser, createdUser);

            // Assert
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(capturedLogEntry, "Log entry should have been captured");
            
            var logParts = capturedLogEntry.Split(';');
            Assert.IsTrue(logParts.Length >= 7, "Log entry should have at least 7 parts");
            Assert.AreEqual("CREATE_USER", logParts[1]);
            Assert.AreEqual(adminId.ToString(), logParts[2]);
            Assert.AreEqual($"{adminFirstName} {adminLastName}", logParts[3]);
            Assert.AreEqual(createdId.ToString(), logParts[4]);
            Assert.AreEqual($"{createdFirstName} {createdLastName}", logParts[5]);
            Assert.AreEqual($"Email={createdEmail}|Admin={createdUser.IsAdmin}", logParts[6]);
        }

        [DataTestMethod]
        [DataRow(1, "Admin", "User", UserRole.Admin, 2, "Test", "User", UserRole.Customer, "FirstName:Test->Tester", DisplayName = "Single change")]
        [DataRow(1, "Admin", "User", UserRole.Admin, 2, "Test", "User", UserRole.Customer, "LastName:User->Smith|City:OldTown->NewTown", DisplayName = "Multiple changes")]
        [DataRow(1, "Admin", "User", UserRole.Admin, 2, "Test", "User", UserRole.Customer, "", DisplayName = "No changes")]
        public void LogUserEdit_CallsWriteLogEntryWithCorrectFormat(
            int adminId, string adminFirstName, string adminLastName, UserRole adminRole,
            int editedId, string editedFirstName, string editedLastName, UserRole editedRole,
            string changedFieldsString)
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            string capturedLogEntry = null;
            mockLoggerAccess.Setup(x => x.WriteLogEntry(It.IsAny<string>()))
                .Callback<string>(entry => capturedLogEntry = entry);
            LoggerLogic.LoggerAccessService = mockLoggerAccess.Object;

            var adminUser = new User { UserID = adminId, FirstName = adminFirstName, LastName = adminLastName, Role = adminRole };
            var editedUser = new User { UserID = editedId, FirstName = editedFirstName, LastName = editedLastName, Role = editedRole };

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
            LoggerLogic.LogUserEdit(adminUser, editedUser, changedFields);

            // Assert
            mockLoggerAccess.Verify(x => x.WriteLogEntry(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(capturedLogEntry, "Log entry should have been captured");
            
            var logParts = capturedLogEntry.Split(';');
            Assert.IsTrue(logParts.Length >= 7, "Log entry should have at least 7 parts");
            Assert.AreEqual("EDIT_USER", logParts[1]);
            Assert.AreEqual(adminId.ToString(), logParts[2]);
            Assert.AreEqual($"{adminFirstName} {adminLastName}", logParts[3]);
            Assert.AreEqual(editedId.ToString(), logParts[4]);
            Assert.AreEqual($"{editedFirstName} {editedLastName}", logParts[5]);
            
            // Verify details format matches expected changes
            string expectedDetails = string.IsNullOrWhiteSpace(changedFieldsString) ? "" : changedFieldsString.Replace(":", "=");
            Assert.AreEqual(expectedDetails, logParts[6]);
        }

        [TestMethod]
        public void ReadLogEntries_ReturnsEntriesOrderedByTimestampDescending()
        {
            // Arrange
            var mockLoggerAccess = new Mock<ILoggerAccess>();
            var mockEntries = new List<LogEntry>
            {
                new LogEntry { Timestamp = "2023-01-01 10:00:00" },
                new LogEntry { Timestamp = "2023-01-03 10:00:00" },
                new LogEntry { Timestamp = "2023-01-02 10:00:00" }
            };
            mockLoggerAccess.Setup(x => x.ReadAllLogEntries()).Returns(mockEntries);
            LoggerLogic.LoggerAccessService = mockLoggerAccess.Object;

            // Act
            var result = LoggerLogic.ReadLogEntries();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("2023-01-03 10:00:00", result[0].Timestamp);
            Assert.AreEqual("2023-01-02 10:00:00", result[1].Timestamp);
            Assert.AreEqual("2023-01-01 10:00:00", result[2].Timestamp);
        }
    }
}