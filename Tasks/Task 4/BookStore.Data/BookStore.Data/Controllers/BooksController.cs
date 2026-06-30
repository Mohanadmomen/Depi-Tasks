using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Data.Models;
using BookStore.Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/books?search=...&categoryId=...&authorId=...&minPrice=...&maxPrice=...&page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] int? authorId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Books
                .AsNoTracking()
                .Where(b => !b.IsDeleted);

            // Filtering
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => b.Title.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice.Value);
            }

            // Pagination
            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookDto
                {
                    Id = b.BookId,
                    Title = b.Title,
                    AuthorName = b.Author!.AuthorName,
                    CategoryName = b.Category!.CategoryName,
                    Price = b.Price
                })
                .ToListAsync();

            return Ok(books);
        }

        // GET: api/books/1
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var bookDto = await _context.Books
                .AsNoTracking()
                .Where(b => b.BookId == id && !b.IsDeleted)
                .Select(b => new BookDto
                {
                    Id = b.BookId,
                    Title = b.Title,
                    AuthorName = b.Author!.AuthorName,
                    CategoryName = b.Category!.CategoryName,
                    Price = b.Price
                })
                .FirstOrDefaultAsync();

            if (bookDto == null) return NotFound(new { message = $"Book with ID {id} not found." });
            return Ok(bookDto);
        }

        // POST: api/books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateBook([FromBody] CreateBookDto bookDto)
        {
            // Verify author and category exist before creation
            var authorExists = await _context.Authors.AnyAsync(a => a.AuthorId == bookDto.AuthorId);
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == bookDto.CategoryId);
            if (!authorExists || !categoryExists)
            {
                return BadRequest(new { message = "Invalid AuthorId or CategoryId." });
            }

            var newBook = new BookStore.Data.Models.Book
            {
                Title = bookDto.Title,
                CategoryId = bookDto.CategoryId,
                AuthorId = bookDto.AuthorId,
                Price = bookDto.Price,
                Stock = bookDto.Stock,
                IsDeleted = false
            };

            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Book added successfully!" });
        }

        // PUT: api/books/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateBook(int id, [FromBody] CreateBookDto bookDto)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id && !b.IsDeleted);
            if (book == null) return NotFound(new { message = $"Book with ID {id} not found." });

            var authorExists = await _context.Authors.AnyAsync(a => a.AuthorId == bookDto.AuthorId);
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == bookDto.CategoryId);
            if (!authorExists || !categoryExists)
            {
                return BadRequest(new { message = "Invalid AuthorId or CategoryId." });
            }

            book.Title = bookDto.Title;
            book.CategoryId = bookDto.CategoryId;
            book.AuthorId = bookDto.AuthorId;
            book.Price = bookDto.Price;
            book.Stock = bookDto.Stock;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Book updated successfully!" });
        }

        // DELETE: api/books/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id && !b.IsDeleted);
            if (book == null) return NotFound(new { message = $"Book with ID {id} not found." });

            // Perform soft delete
            book.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Book deleted successfully!" });
        }
    }
}