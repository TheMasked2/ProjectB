using Microsoft.Data.Sqlite;
using Dapper;

public class UserAccess : IUserAccess
{
    private readonly SqliteConnection _connection;
    private readonly string Table = "USERS";

    public UserAccess()
    {
        _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    }

    public void AddUser(User user)
    {
        string sql = $@"INSERT INTO {Table} 
                        (FirstName, LastName, Country, City, Email, Password, PhoneNumber, BirthDate, AccCreatedAt, IsAdmin) 
                        VALUES 
                        (@FirstName, @LastName, @Country, @City, @EmailAddress, @Password, @PhoneNumber, @BirthDate, @AccCreatedAt, @IsAdmin)";
        _connection.Execute(sql, user);
    }

    public User GetUserInfoByEmail(string email)
    {
        string sql = $@"SELECT 
                        UserID AS UserID,
                        FirstName AS FirstName,
                        LastName AS LastName,
                        Country AS Country,
                        City AS City,
                        Email AS EmailAddress,
                        Password AS Password,
                        PhoneNumber AS PhoneNumber,
                        BirthDate AS BirthDate,
                        AccCreatedAt AS AccCreatedAt
                    FROM {Table}
                    WHERE Email = @EmailAddress";

        return _connection.QueryFirstOrDefault<User>(sql, new { EmailAddress = email });
    }

    public User GetUserById(int userId)
    {
        string sql = $@"SELECT 
                        UserID AS UserID,
                        FirstName AS FirstName,
                        LastName AS LastName,
                        Country AS Country,
                        City AS City,
                        Email AS EmailAddress,
                        Password AS Password,
                        PhoneNumber AS PhoneNumber,
                        BirthDate AS BirthDate,
                        AccCreatedAt AS AccCreatedAt,
                        IsAdmin AS IsAdmin 
                        FROM USERS 
                        WHERE UserID = @UserID";

        return _connection.QuerySingleOrDefault<User>(sql, new { @UserID = userId });
    }

    public User Login(string email, string password)
    {
        string sql = $@"SELECT 
                        UserID AS UserId,
                        FirstName AS FirstName,
                        LastName AS LastName,
                        Country AS Country,
                        City AS City,
                        Email AS EmailAddress,
                        Password AS Password,
                        PhoneNumber AS PhoneNumber,
                        BirthDate AS BirthDate,
                        AccCreatedAt AS AccCreatedAt,
                        IsAdmin AS IsAdmin 
                    FROM {Table} 
                    WHERE Email = @EmailAddress AND Password = @Password";

        return _connection.QueryFirstOrDefault<User>(sql, new { EmailAddress = email, Password = password });
    }

    public List<User> GetAllUsers()
    {
        string sql = $@"SELECT 
                        UserID AS UserId,
                        FirstName AS FirstName,
                        LastName AS LastName,
                        Country AS Country,
                        City AS City,
                        Email AS EmailAddress,
                        Password AS Password,
                        PhoneNumber AS PhoneNumber,
                        BirthDate AS BirthDate,
                        AccCreatedAt AS AccCreatedAt,
                        IsAdmin AS IsAdmin
                    FROM {Table}";

        return _connection.Query<User>(sql).ToList();
    }

    public void UpdateUser(User user)
    {
        string sql = $@"UPDATE {Table} 
                        SET FirstName = @FirstName,
                            LastName = @LastName, 
                            Country = @Country,
                            City = @City,
                            Email = @EmailAddress,
                            Password = @Password,
                            PhoneNumber = @PhoneNumber,
                            BirthDate = @BirthDate,
                            IsAdmin = @IsAdmin
                        WHERE UserID = @UserID";

        _connection.Execute(sql, user);
    }

    public int GetHighestUserId()
    {
        string sql = $@"SELECT MAX(UserID) FROM {Table}";
        var result = _connection.ExecuteScalar<int?>(sql);
        return result ?? 0;
    }
}