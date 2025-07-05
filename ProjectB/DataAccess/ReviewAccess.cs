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
                        (FlightID, UserID, Rating, Comment) 
                        VALUES (@FlightID, @UserID, @Rating, @Comment)";
        _connection.Execute(sql, review);
    }

    public override void Update(ReviewModel review)
    {
        string sql = @$"UPDATE {Table} 
                        SET FlightID = @FlightID, 
                            UserID = @UserID, 
                            Rating = @Rating, 
                            Comment = @Comment 
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
}