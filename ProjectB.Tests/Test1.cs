using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace ProjectB;

[TestClass]
[DoNotParallelize]
public class SessionManagerTests
{
    [TestInitialize]
    public void Setup()
    {
        SessionManager.Logout();
    }

    [TestCleanup]
    public void Cleanup()
    {
        SessionManager.Logout();
    }

    [DataTestMethod]
    [DataRow("Cheng", true)]
    [DataRow("Mario", true)]
    [DataRow("Mario1", true)]
    [DataRow("Mario2", true)]
    [DataRow("Mario3", true)]
    [DataRow("Mario4", true)]
    [DataRow("Mario5", true)]
    [DataRow("Mario6", true)]
    [DataRow("Mario7", true)]
    [DataRow("Mario8", true)]
    [DataRow("Mario9", true)]
    [DataRow("Mario10", true)]
    [DataRow("Mario11", true)]
    
    public void SetCurrentUser_ValidUsernames_SetsCorrectUsernameAndLoginState(string firstname, bool expectedLoginState)
    {
        // Arrange
        var user = new User { FirstName = firstname };

        // Act
        SessionManager.SetCurrentUser(user);

        // Assert
        Assert.AreEqual(firstname, SessionManager.CurrentUser.FirstName);
        Assert.AreEqual(expectedLoginState, SessionManager.IsLoggedIn());
        Assert.IsTrue((DateTime.Now - SessionManager.LoginTime).TotalSeconds < 1);
    }
}