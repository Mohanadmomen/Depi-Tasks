using System.ComponentModel.DataAnnotations;

namespace BookStore.Data.DTOs;

// What the user sends to log in
public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

// What the user sends to create a new account
public record RegisterDto(
    [Required] string FullName,
    [Required][EmailAddress] string Email,
    [Required] string City, // <-- Added City here!
    [Required][MinLength(6)] string Password
);

// What the API sends back after a successful login
public record AuthResponseDto(
    string Token,
    string Email,
    string Role
);

// --- ADMIN DASHBOARD DTOS (Kept safe!) ---

// The summary statistics for the top of the Admin Dashboard
public record DashboardStatsDto(
    int TotalBooks,
    int TotalCategories,
    int TotalOrders,
    decimal TotalRevenue
);

// What the admin sees when looking at system-wide orders
public record AdminOrderViewDto(
    int OrderId,
    string CustomerEmail,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status
);