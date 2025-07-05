using Dapper;
using Microsoft.Data.Sqlite;
using ProjectB.DataAccess;

public abstract class GenericAccess<TModel, TKey> : IGenericAccess<TModel, TKey> where TModel : class
{
    protected readonly SqliteConnection _connection = new SqliteConnection($"Data Source=DataSources/database.db");
    protected abstract string Table { get; }
    protected abstract string PrimaryKey { get; }

    public virtual TModel? GetById(TKey id)
    {
        string sql = $"SELECT * FROM {Table} WHERE {PrimaryKey} = @Id";
        var paramaters = new { Id = id };
        return _connection.QuerySingleOrDefault<TModel>(sql, paramaters);
    }

    public virtual List<TModel>? GetAll()
    {
        string sql = $"SELECT * FROM {Table}";
        return _connection.Query<TModel>(sql).ToList();
    }

    public virtual void Insert(TModel model)
    {
        throw new NotImplementedException();
    }

    public virtual void Update(TModel model)
    {
        throw new NotImplementedException();
    }

    public virtual void Delete(TKey id)
    {
        string sql = $"DELETE FROM {Table} WHERE {PrimaryKey} = @Id";
        var paramaters = new { Id = id };
        _connection.Execute(sql, paramaters);
    }
}