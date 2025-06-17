using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public class ReviewAcces
{
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");

    private static string Table = "REVIEWS";

    public void AddReview(ReviewModel review)
    {
        string sql = @$"INSERT INTO {Table} (UserID, FlightID, Content, Rating, CreatedAt) 
                       VALUES (@UserID, @FlightID, @Content, @Rating, @CreatedAt)";
        _connection.Execute(sql, review);
    }

    public List<ReviewModel> GetReviewsByFlight(int flightId)
    {
        string sql = @$"SELECT * FROM {Table} WHERE FlightID = @FlightId";
        return _connection.Query<ReviewModel>(sql, new { FlightId = flightId }).ToList();
    }

    public List<ReviewModel> GetAllReviews()
    {
        string sql = @$"SELECT * FROM {Table}";
        return _connection.Query<ReviewModel>(sql).ToList();
    }

    public List<ReviewModel> GetReviewsByUser(int userId)
    {
        string sql = @$"SELECT * FROM {Table} WHERE UserID = @UserId";
        return _connection.Query<ReviewModel>(sql, new { UserId = userId }).ToList();
    }
}