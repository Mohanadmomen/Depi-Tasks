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
    [Authorize] // All endpoints require authentication
    public class AuthorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/authors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
        {
            return await _context.Authors
                .AsNoTracking()
                .Select(a => new AuthorDto(a.AuthorId, a.AuthorName))
                .ToListAsync();
        }

        // GET: api/authors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetAuthor(int id)
        {
            var author = await _context.Authors
                .AsNoTracking()
                .Where(a => a.AuthorId == id)
                .Select(a => new AuthorDto(a.AuthorId, a.AuthorName))
                .FirstOrDefaultAsync();

            if (author == null)
            {
                return NotFound(new { message = $"Author with ID {id} not found." });
            }

            return Ok(author);
        }

        // POST: api/authors
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admins can modify
        public async Task<ActionResult> CreateAuthor([FromBody] CreateAuthorDto dto)
        {
            var author = new Author
            {
                AuthorName = dto.Name
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuthor), new { id = author.AuthorId }, new AuthorDto(author.AuthorId, author.AuthorName));
        }

        // PUT: api/authors/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateAuthor(int id, [FromBody] CreateAuthorDto dto)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound(new { message = $"Author with ID {id} not found." });
            }

            author.AuthorName = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Author updated successfully!" });
        }

        // DELETE: api/authors/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound(new { message = $"Author with ID {id} not found." });
            }

            // Prevent deleting author if books belong to it (comply with DeleteBehavior.Restrict or business rules)
            if (author.Books.Any(b => !b.IsDeleted))
            {
                return BadRequest(new { message = "Cannot delete author because they have books in the bookstore." });
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Author deleted successfully!" });
        }
    }
}
