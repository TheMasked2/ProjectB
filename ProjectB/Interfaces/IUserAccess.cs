public interface IUserAccess
{
    User GetUserById(int userId);
    List<User> GetAllUsers();
    void AddUser(User user);
    void UpdateUser(User user);
    int GetHighestUserId();
    User Login(string email, string password);
    User GetUserInfoByEmail(string email);
    User GetUserInfoByID(int userId);
}