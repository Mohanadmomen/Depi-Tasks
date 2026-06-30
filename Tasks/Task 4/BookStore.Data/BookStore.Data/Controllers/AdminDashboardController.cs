using BookStore.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;

namespace BookStore.API.Controllers;

[Route("api/admin/dashboard")]
[ApiController]
[Authorize(Roles = "Admin")] // Locks this ENTIRE controller down to Admins only!
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminDashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var totalBooks = await _context.Books.CountAsync(b => !b.IsDeleted);
        var totalCategories = await _context.Categories.CountAsync();
        var totalOrders = await _context.Purchases.CountAsync();
        
        // Sum total amount paid for all purchase items
        var totalRevenue = await _context.PurchaseItems
            .SumAsync(pi => pi.Quantity * pi.UnitPrice);

        var stats = new DashboardStatsDto(
            TotalBooks: totalBooks,
            TotalCategories: totalCategories,
            TotalOrders: totalOrders,
            TotalRevenue: totalRevenue
        );

        return Ok(stats);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<AdminOrderViewDto>>> GetAllSystemOrders()
    {
        // Requirement 10 & 30: Admins must be able to see ALL orders placed by ALL customers
        var orders = await _context.Purchases
            .Include(p => p.Customer)
            .Include(p => p.Items)
            .AsNoTracking()
            .OrderByDescending(p => p.PurchaseDate)
            .Select(p => new AdminOrderViewDto(
                p.PurchaseId,
                p.Customer!.Email,
                p.PurchaseDate,
                p.Items.Sum(i => i.Quantity * i.UnitPrice),
                "Completed"
            ))
            .ToListAsync();

        return Ok(orders);
    }
}