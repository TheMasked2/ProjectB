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
                        AccCreatedAt,
                        Role,
                        FirstTimeDiscount)
                        VALUES 
                        (@FirstName, 
                        @LastName, 
                        @Country, 
                        @City, 
                        @Email,
                        @Password, 
                        @PhoneNumber, 
                        @BirthDate, 
                        @AccCreatedAt,
                        @Role,
                        @FirstTimeDiscount)";
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
                            BirthDate = @BirthDate,
                            Role = @Role,
                            FirstTimeDiscount = @FirstTimeDiscount
                        WHERE UserID = @UserID";
        _connection.Execute(sql, model);
    }

    public User? GetUserInfoByEmail(string email)
    {
        string sql = $@"SELECT * FROM {Table}
                        WHERE Email = @Email";
        var parameters = new { Email = email };
        return _connection.QuerySingleOrDefault<User>(sql, parameters);
    }

    public User? Login(string email, string password)
    {
        string sql = $@"SELECT * FROM {Table} 
                    WHERE Email = @Email AND Password = @Password";
        var parameters = new { Email = email, Password = password };
        return _connection.QueryFirstOrDefault<User>(sql, parameters);
    }
}