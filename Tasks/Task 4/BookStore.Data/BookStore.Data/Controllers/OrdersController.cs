using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Data.Models;
using BookStore.Data.DTOs;
using System.Security.Claims;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requirement 16
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
            
            if (customer == null)
            {
                _logger.LogWarning("PlaceOrder failed: Customer not found for authenticated user {Email}", userEmail);
                return Unauthorized(new { message = "User not found in system." });
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                return BadRequest(new { message = "Order must contain at least one item." });
            }

            // Using database transaction to ensure order placement is atomic
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchase = new Purchase
                {
                    CustomerId = customer.CustomerId,
                    PurchaseDate = DateTime.UtcNow,
                    Items = new List<PurchaseItem>()
                };

                foreach (var item in dto.Items)
                {
                    var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == item.BookId && !b.IsDeleted);
                    if (book == null)
                    {
                        return BadRequest(new { message = $"Book with ID {item.BookId} does not exist." });
                    }

                    if (book.Stock < item.Quantity)
                    {
                        return BadRequest(new { message = $"Insufficient stock for '{book.Title}'. Requested: {item.Quantity}, Available: {book.Stock}." });
                    }

                    // Deduct stock
                    book.Stock -= item.Quantity;

                    purchase.Items.Add(new PurchaseItem
                    {
                        BookId = book.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = book.Price
                    });
                }

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order placed successfully. PurchaseID: {PurchaseId}, Customer: {Email}, ItemsCount: {ItemsCount}", 
                    purchase.PurchaseId, customer.Email, purchase.Items.Count);

                return Ok(new { message = "Order placed successfully!", purchaseId = purchase.PurchaseId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while placing order for customer {Email}", customer.Email);
                throw; // Caught by global exception middleware
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetOrders()
        {
            var isAdmin = User.IsInRole("Admin");
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;

            var query = _context.Purchases
                .Include(p => p.Customer)
                .Include(p => p.Items)
                .ThenInclude(i => i.Book)
                .AsNoTracking();

            if (!isAdmin)
            {
                query = query.Where(o => o.Customer!.Email == userEmail);
            }

            var orders = await query
                .Select(p => new PurchaseDto(
                    p.PurchaseId,
                    p.CustomerId,
                    p.Customer!.Email,
                    p.PurchaseDate,
                    p.Items.Select(i => new PurchaseItemDto(
                        i.PurchaseItemId,
                        i.BookId,
                        i.Book!.Title,
                        i.Quantity,
                        i.UnitPrice
                    )).ToList(),
                    p.Items.Sum(i => i.Quantity * i.UnitPrice)
                ))
                .ToListAsync();

            return Ok(orders);
        }
    }
}