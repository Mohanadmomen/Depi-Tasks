using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookStore.Data.DTOs;
using Microsoft.IdentityModel.Tokens;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BookStore.Data.Models;

namespace BookStore.Data.Services;

// The contract our controller will talk to
public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<bool> RegisterAsync(RegisterDto registerDto);
}

// The actual worker bee doing the heavy lifting
public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Customer> _passwordHasher;

    public AuthService(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
        _passwordHasher = new PasswordHasher<Customer>();
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == loginDto.Email);

        if (customer == null)
        {
            return null; // User not found
        }

        var result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, loginDto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null; // Invalid password
        }

        var token = GenerateJwtToken(customer.Email, customer.Role);
        return new AuthResponseDto(token, customer.Email, customer.Role);
    }

    public async Task<bool> RegisterAsync(RegisterDto registerDto)
    {
        // Enforce Unique Customer Email (Requirement #103 & #166)
        var exists = await _context.Customers.AnyAsync(c => c.Email == registerDto.Email);
        if (exists)
        {
            return false;
        }

        var newCustomer = new Customer
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            City = registerDto.City,
            Role = "Customer" // Default role
        };

        newCustomer.PasswordHash = _passwordHasher.HashPassword(newCustomer, registerDto.Password);

        _context.Customers.Add(newCustomer);
        await _context.SaveChangesAsync();

        return true;
    }

    private string GenerateJwtToken(string email, string role)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "SuperSecretKey12345678901234567890";
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role) // This embeds "Admin" or "Customer" into the token!
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["ExpiryInDays"] ?? "7")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}