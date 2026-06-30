using Microsoft.AspNetCore.Mvc;
using BookStore.Data.DTOs;
using BookStore.Data.Services;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            var response = await _authService.LoginAsync(loginDto);
            if (response == null)
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", loginDto.Email);
                return Unauthorized(new { message = "Invalid email or password." });
            }

            _logger.LogInformation("Successful login for email: {Email}", loginDto.Email);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", registerDto.Email);

            var success = await _authService.RegisterAsync(registerDto);
            if (!success)
            {
                _logger.LogWarning("Failed registration: Email {Email} already exists", registerDto.Email);
                return BadRequest(new { message = "Email is already in use." });
            }

            _logger.LogInformation("Successful registration for email: {Email}", registerDto.Email);
            return Ok(new { message = "Registration successful!" });
        }
    }
}