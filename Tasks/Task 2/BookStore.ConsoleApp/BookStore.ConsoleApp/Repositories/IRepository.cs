namespace BookStore.ConsoleApp.Repositories
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        void Remove(T entity);
        List<T> GetAll();
        T? GetById(int id);
    }
}