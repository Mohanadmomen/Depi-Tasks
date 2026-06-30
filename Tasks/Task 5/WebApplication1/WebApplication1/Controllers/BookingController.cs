using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int showtimeId)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h!.Cinema)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                TempData["ErrorMessage"] = "Showtime not found.";
                return RedirectToAction("Index", "Home");
            }

            if (showtime.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "This showtime has already started. Booking is closed.";
                return RedirectToAction("MovieDetails", "Home", new { id = showtime.MovieId });
            }

            // Calculate remaining seats
            int bookedSeats = await _context.Bookings
                .Where(b => b.ShowtimeId == showtimeId && !b.IsCancelled)
                .SumAsync(b => b.SeatsBooked);

            int capacity = showtime.Hall?.Capacity ?? 0;
            int availableSeats = capacity - bookedSeats;

            if (availableSeats <= 0)
            {
                TempData["ErrorMessage"] = "This showtime is sold out.";
                return RedirectToAction("MovieDetails", "Home", new { id = showtime.MovieId });
            }

            ViewBag.AvailableSeats = availableSeats;

            var model = new Booking
            {
                ShowtimeId = showtimeId,
                Showtime = showtime,
                SeatsBooked = 1,
                TotalPrice = showtime.Price
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(Booking model)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == model.ShowtimeId);

            if (showtime == null)
            {
                TempData["ErrorMessage"] = "Showtime not found.";
                return RedirectToAction("Index", "Home");
            }

            if (showtime.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "This showtime has already started. Booking is closed.";
                return RedirectToAction("MovieDetails", "Home", new { id = showtime.MovieId });
            }

            // Calculate remaining seats
            int bookedSeats = await _context.Bookings
                .Where(b => b.ShowtimeId == model.ShowtimeId && !b.IsCancelled)
                .SumAsync(b => b.SeatsBooked);

            int capacity = showtime.Hall?.Capacity ?? 0;
            int availableSeats = capacity - bookedSeats;

            if (model.SeatsBooked <= 0)
            {
                ModelState.AddModelError(nameof(model.SeatsBooked), "You must book at least 1 seat.");
            }
            else if (model.SeatsBooked > availableSeats)
            {
                ModelState.AddModelError(nameof(model.SeatsBooked), $"Only {availableSeats} seats are available.");
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var booking = new Booking
                {
                    ShowtimeId = model.ShowtimeId,
                    UserId = userId,
                    SeatsBooked = model.SeatsBooked,
                    BookingTime = DateTime.Now,
                    TotalPrice = showtime.Price * model.SeatsBooked,
                    IsCancelled = false
                };

                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully booked {model.SeatsBooked} tickets for {showtime.Movie?.Title}!";
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.AvailableSeats = availableSeats;
            model.Showtime = showtime;
            TempData["ErrorMessage"] = "Failed to book tickets. Please correct the validation errors.";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Showtime)
                    .ThenInclude(s => s!.Movie)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s!.Hall)
                        .ThenInclude(h => h!.Cinema)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var booking = await _context.Bookings
                .Include(b => b.Showtime)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.IsCancelled)
            {
                TempData["ErrorMessage"] = "This booking is already cancelled.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.Showtime != null && booking.Showtime.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Cannot cancel booking because the showtime has already started.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.IsCancelled = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your booking was successfully cancelled.";
            return RedirectToAction(nameof(MyBookings));
        }
    }
}
