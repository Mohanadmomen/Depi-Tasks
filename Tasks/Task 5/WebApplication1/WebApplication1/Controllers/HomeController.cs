using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch movies currently showing (with categories)
            var movies = await _context.Movies
                .Include(m => m.Category)
                .Take(6)
                .ToListAsync();

            return View(movies);
        }

        public async Task<IActionResult> Movies(int? categoryId)
        {
            ViewData["Categories"] = await _context.Categories.ToListAsync();
            ViewData["SelectedCategory"] = categoryId;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId.Value);
            }

            var movies = await moviesQuery.ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> MovieDetails(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.StartTime > DateTime.Now).OrderBy(s => s.StartTime))
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h!.Cinema)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return RedirectToAction("Error", new { id = 404 });
            }

            // Also load total booked seats for each showtime to display remaining seats
            var showtimeBookings = await _context.Bookings
                .Where(b => b.Showtime!.MovieId == id && !b.IsCancelled)
                .GroupBy(b => b.ShowtimeId)
                .Select(g => new { ShowtimeId = g.Key, BookedSeats = g.Sum(b => b.SeatsBooked) })
                .ToDictionaryAsync(x => x.ShowtimeId, x => x.BookedSeats);

            ViewBag.BookedSeats = showtimeBookings;

            return View(movie);
        }

        public async Task<IActionResult> Cinemas()
        {
            var cinemas = await _context.Cinemas
                .Include(c => c.Halls)
                .ToListAsync();

            return View(cinemas);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            if (id == 404)
            {
                ViewData["ErrorCode"] = "404";
                ViewData["ErrorMessage"] = "Page Not Found";
                ViewData["ErrorDetails"] = "The page you are looking for might have been removed, had its name changed, or is temporarily unavailable.";
                return View("CustomError");
            }
            else if (id == 500)
            {
                ViewData["ErrorCode"] = "500";
                ViewData["ErrorMessage"] = "Internal Server Error";
                ViewData["ErrorDetails"] = "An unexpected error occurred on the server. Our technical team has been notified.";
                return View("CustomError");
            }

            var requestModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return View(requestModel);
        }
    }
}
