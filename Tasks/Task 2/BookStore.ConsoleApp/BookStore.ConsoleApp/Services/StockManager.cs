using BookStore.ConsoleApp.Models;

namespace BookStore.ConsoleApp.Services
{
    public class StockManager
    {
        
        public event Action<Book>? OnOutOfStock;

        public bool TrySellBook(Book book, int quantityToSell)
        {
            if (book.Stock < quantityToSell)
            {
                return false; 
            }

            book.Stock -= quantityToSell;

           
            if (book.Stock == 0)
            {
                OnOutOfStock?.Invoke(book);
            }

            return true;
        }
    }
}