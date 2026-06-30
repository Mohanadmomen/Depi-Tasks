using BookStore.ConsoleApp.Models;

namespace BookStore.ConsoleApp.Services
{
    public class BookService
    {
        
        public List<Book> ApplyCustomFilter(List<Book> allBooks, Func<Book, bool> rule)
        {
            return allBooks.Where(rule).ToList();
        }

        public void ApplyBulkPriceAdjustment(List<Book> books, decimal multiplier)
        {
            foreach (var book in books)
            {
                book.Price *= multiplier;
            }
        }
    }
}