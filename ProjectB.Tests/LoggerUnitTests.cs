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
        [DataRow(1, "Admin", "User", 2, "Test", "User", "test@example.com", false, true)]
        [DataRow(3, "Admin", "Smith", 4, "Bob", "Jones", "bob@example.com", true, true)]
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

        [DataTestMethod]
        [DataRow(1, "Admin", "User", 2, "Test", "User", "LastName:User->Smith|City:OldTown->NewTown", true)]
        [DataRow(5, "Super", "Visor", 6, "Another", "Editor", "PhoneNumber:12345->67890|IsAdmin:False->True", false)]
        [DataRow(2, "Jane", "Doe", 10, "John", "Public", "FirstName:John->Jonathan|Country:USA->Canada|PhoneNumber:5550100->5550199", true)]
        [DataRow(1, "Root", "User", 1, "Root", "User", "IsAdmin:True->False", true)]
        [DataRow(7, "Manager", "Alpha", 15, "Employee", "Beta", "LastName:Beta->Gamma|City:Metro->Suburbia|Country:UK->France", true)]
        [DataRow(9, "Temp", "Admin", 25, "Contractor", "Gamma", "FirstName:Contractor->Consultant|PhoneNumber:N/A->111222333", false)]
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
    }
}