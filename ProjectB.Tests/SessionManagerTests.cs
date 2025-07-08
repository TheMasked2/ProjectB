namespace ProjectB.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class SessionManagerTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            SessionManager.Logout();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // make sure ur logged out after running each testini
            SessionManager.Logout();
        }

        [TestMethod]
        public void SetCurrentUser_SetsCurrentUserAndLoginTime()
        {
            // Arrange
            var user = new User { UserID = 1, FirstName = "Devin", LastName = "Test" };

            // Act
            SessionManager.SetCurrentUser(user);

            // Assert

            // check if the curent user is set correctly
            Assert.AreEqual(user, SessionManager.CurrentUser);
            // checks if the login time is correct
            Assert.IsTrue((DateTime.Now - SessionManager.LoginTime).TotalSeconds < 2);
        }

        [TestMethod]
        public void Logout_ClearsCurrentUserAndResetsLoginTime()
        {
            // Arrange
            var user = new User { UserID = 2, FirstName = "Devin", LastName = "Testing"};
            SessionManager.SetCurrentUser(user);

            // Act
            SessionManager.Logout();

            // Assert
            Assert.IsNull(SessionManager.CurrentUser);
            Assert.AreEqual(DateTime.MinValue, SessionManager.LoginTime);
        }

        [TestMethod]
        public void IsLoggedIn_ReturnsTrueWhenUserIsSet()
        {
            // Arrange
            var user = new User { UserID = 3, FirstName = "Test", LastName = "Devin" };
            SessionManager.SetCurrentUser(user);

            // Act
            var result = SessionManager.IsLoggedIn();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsLoggedIn_ReturnsFalseWhenNoUserIsSet()
        {
            // Arrange
            SessionManager.Logout();

            // Act
            var result = SessionManager.IsLoggedIn();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SetCurrentUser_OverridesPreviousUser()
        {
            // Arrange
            var user1 = new User { UserID = 1, FirstName = "Devin", LastName = "N" };
            var user2 = new User { UserID = 2, FirstName = "N", LastName = "Devin" };
            SessionManager.SetCurrentUser(user1);

            // Act
            SessionManager.SetCurrentUser(user2);

            // Assert
            Assert.AreEqual(user2, SessionManager.CurrentUser);
        }

        [TestMethod]
        public void SetGuestUser_SetsGuestUserAndLoginTime()
        {
            // Act
            SessionManager.SetGuestUser();

            // Assert
            Assert.IsNotNull(SessionManager.CurrentUser);
            Assert.AreEqual(-1, SessionManager.CurrentUser.UserID);
            Assert.AreEqual("Guest", SessionManager.CurrentUser.FirstName);
            Assert.AreEqual("User", SessionManager.CurrentUser.LastName);
            Assert.AreEqual(UserRole.Guest, SessionManager.CurrentUser.Role);
            Assert.IsTrue(SessionManager.CurrentUser.IsGuest);
            Assert.IsTrue((DateTime.Now - SessionManager.LoginTime).TotalSeconds < 2);
        }
    }
}