using Microsoft.Data.Sqlite;
using Dapper;

public static class UserAccess
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private static string Table = "USERS";

    public static void AddUser(User user)
    {
        string sql = $@"INSERT INTO {Table} 
                        (FirstName, LastName, Country, City, Email, Password, PhoneNumber, BirthDate, AccCreatedAt, IsAdmin) 
                        VALUES 
                        (@FirstName, @LastName, @Country, @City, @EmailAddress, @Password, @PhoneNumber, @BirthDate, @AccCreatedAt, @IsAdmin)";
        _connection.Execute(sql, user);
    }

    public static void RemoveUser(string email)
    {
        string sql = $"DELETE FROM {Table} WHERE Email = @EmailAddress";
        _connection.Execute(sql, new { EmailAddress = email });
    }

    public static User GetUserInfoByEmail(string email)
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
                    FROM {Table}
                    WHERE Email = @EmailAddress";

        return _connection.QueryFirstOrDefault<User>(sql, new { EmailAddress = email });
    }

    public static User GetUserInfoByID(int userId)
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
        if (_connection.QuerySingleOrDefault<User>(sql, new { @UserID = userId }) == null)
        {
            return null;
        }
        else
        {
            var user = _connection.QuerySingleOrDefault<User>(sql, new { @UserID = userId });
            return user;
        }
    }

    public static User Login(string email, string password)
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

    public static List<User> GetAllUsers()
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

    public static void UpdateUser(User user)
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

    public static int GetHighestUserId()
    {
        string sql = $@"SELECT MAX(UserID) FROM {Table}";
        var result = _connection.ExecuteScalar<int?>(sql);
        return result ?? 0;
    }
}