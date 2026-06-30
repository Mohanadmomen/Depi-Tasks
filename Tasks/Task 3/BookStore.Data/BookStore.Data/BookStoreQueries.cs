using Microsoft.EntityFrameworkCore;
using BookStore.Data.Models;

namespace BookStore.Data
{
    public class BookStoreQueries
    {
        private readonly AppDbContext _context;

        public BookStoreQueries(AppDbContext context)
        {
            _context = context;
        }

     
        public async Task<List<Book>> GetAllBooksEfficientlyAsync()
        {
            return await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.Author)
                .ToListAsync();
        }

        // TASK 3.5: Get top 5 best-selling books
        public async Task<List<object>> GetTop5BestSellersAsync()
        {
            return await _context.PurchaseItems
                .AsNoTracking()
                .GroupBy(pi => new { pi.BookId, pi.Book!.Title })
                .Select(g => new
                {
                    BookTitle = g.Key.Title,
                    TotalSold = g.Sum(pi => pi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync<object>();
        }

        public async Task<List<Book>> GetExpensiveBooksAsync()
        {
            decimal averagePrice = await _context.Books.AsNoTracking().AverageAsync(b => b.Price);

            return await _context.Books
                .AsNoTracking()
                .Where(b => b.Price > averagePrice)
                .ToListAsync();
        }

        public async Task<List<Book>> SearchByTitleAsync(string keyword)
        {
            return await _context.Books
                .AsNoTracking()
                .Where(b => EF.Functions.Like(b.Title, $"%{keyword}%"))
                .ToListAsync();
        }

        public async Task<List<Book>> GetBooksPaginatedAsync(int pageNumber, int pageSize)
        {
            return await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.BookId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}