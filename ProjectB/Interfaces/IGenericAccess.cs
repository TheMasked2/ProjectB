namespace ProjectB.DataAccess
{
    public interface IGenericAccess<TModel, TKey> where TModel : class
    {
        TModel? GetById(TKey id);
        List<TModel>? GetAll();
        void Delete(TKey id);
        void Insert(TModel item);
        void Update(TModel item);
    }
}