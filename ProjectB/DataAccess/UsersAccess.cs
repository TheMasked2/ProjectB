using Microsoft.Data.Sqlite;
using Dapper;
using ProjectB.DataAccess;

public class UserAccess : GenericAccess<User, int>, IUserAccess
{
    protected override string PrimaryKey => "UserID";
    protected override string Table => "USERS";

    public override void Insert(User model)
    {
        string sql = $@"INSERT INTO {Table} 
                        (FirstName,
                        LastName, 
                        Country, 
                        City, 
                        Email, 
                        Password, 
                        PhoneNumber, 
                        BirthDate, 
                        AccCreatedAt)
                        VALUES 
                        (@FirstName, 
                        @LastName, 
                        @Country, 
                        @City, 
                        @Email, 
                        @Password, 
                        @PhoneNumber, 
                        @BirthDate, 
                        @AccCreatedAt)";
        _connection.Execute(sql, model);
    }

    public override void Update(User model)
    {
        string sql = $@"UPDATE {Table}
                        SET
                            FirstName = @FirstName,
                            LastName = @LastName,
                            Country = @Country,
                            City = @City,
                            Email = @Email,
                            Password = @Password,
                            PhoneNumber = @PhoneNumber,
                            BirthDate = @BirthDate
                        WHERE UserID = @UserID";
        _connection.Execute(sql, model);
    }

    public User? GetUserInfoByEmail(string email)
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
        var parameters = new { EmailAddress = email };
        return _connection.QuerySingleOrDefault<User>(sql, parameters);
    }

    public User? Login(string email, string password)
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
                        Role AS Role 
                    FROM {Table} 
                    WHERE Email = @EmailAddress AND Password = @Password";
        var parameters = new { EmailAddress = email, Password = password };
        return _connection.QueryFirstOrDefault<User>(sql, parameters);
    }

    public int GetHighestUserId()
    {
        string sql = $@"SELECT MAX(UserID) FROM {Table}";
        return _connection.ExecuteScalar<int>(sql);
    }

}