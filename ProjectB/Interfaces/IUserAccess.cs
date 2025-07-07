namespace ProjectB.DataAccess
{
    public interface IUserAccess : IGenericAccess<User, int>
    {
        User? Login(string email, string password);
        User? GetUserInfoByEmail(string email);
    }
}
