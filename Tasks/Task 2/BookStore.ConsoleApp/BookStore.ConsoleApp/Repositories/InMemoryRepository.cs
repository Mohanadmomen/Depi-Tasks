using BookStore.ConsoleApp.Models;

namespace BookStore.ConsoleApp.Repositories
{
    
    public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly List<T> _data = new();

        public void Add(T entity)
        {
            _data.Add(entity);
        }

        public void Remove(T entity)
        {
            _data.Remove(entity);
        }

        public List<T> GetAll()
        {
            return _data;
        }

        public virtual T? GetById(int id)
        {
            return _data.FirstOrDefault(x => x.Id == id);
        }
    }
}