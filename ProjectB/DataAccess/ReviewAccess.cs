using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public class ReviewAccess : GenericAccess<ReviewModel, int>, IReviewAccess
{
    protected override string PrimaryKey => "ReviewID";
    protected override string Table => "REVIEWS";

    public override void Insert(ReviewModel review)
    {
        string sql = @$"INSERT INTO {Table} 
                        (FlightID, UserID, Rating, Content, CreatedAt) 
                        VALUES (@FlightID, @UserID, @Rating, @Content, @CreatedAt)";
        _connection.Execute(sql, review);
    }

    public override void Update(ReviewModel review)
    {
        string sql = @$"UPDATE {Table} 
                        SET FlightID = @FlightID, 
                            UserID = @UserID, 
                            Rating = @Rating, 
                            Content = @Content,
                            CreatedAt = @CreatedAt
                        WHERE ReviewID = @ReviewID";
        _connection.Execute(sql, review);
    }

    public List<ReviewModel> GetReviewsByFlight(int flightId)
    {
        string sql = @$"SELECT * FROM {Table} WHERE FlightID = @FlightId";
        return _connection.Query<ReviewModel>(sql, new { FlightId = flightId }).ToList();
    }

    public List<ReviewModel> GetReviewsByUser(int userId)
    {
        string sql = @$"SELECT * FROM {Table} WHERE UserID = @UserId";
        return _connection.Query<ReviewModel>(sql, new { UserId = userId }).ToList();
    }

    public void DeleteReviewsByFlightID(int flightId)
    {
        string sql = @$"DELETE FROM {Table} WHERE FlightID = @FlightId";
        _connection.Execute(sql, new { FlightId = flightId });
    }
}