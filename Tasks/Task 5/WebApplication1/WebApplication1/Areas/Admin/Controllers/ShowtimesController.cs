using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ShowtimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowtimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h!.Cinema)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
            return View(showtimes);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Showtime showtime)
        {
            if (ModelState.IsValid)
            {
                await _context.Showtimes.AddAsync(showtime);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Showtime scheduled successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(showtime.MovieId, showtime.HallId);
            TempData["ErrorMessage"] = "Failed to schedule showtime. Please fix errors.";
            return View(showtime);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
            {
                TempData["ErrorMessage"] = "Showtime not found.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(showtime.MovieId, showtime.HallId);
            return View(showtime);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Showtime showtime)
        {
            if (id != showtime.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(showtime);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Showtime updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ShowtimeExists(showtime.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(showtime.MovieId, showtime.HallId);
            TempData["ErrorMessage"] = "Failed to update showtime. Please fix errors.";
            return View(showtime);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h!.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
            {
                TempData["ErrorMessage"] = "Showtime not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(showtime);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Showtime deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(int? selectedMovieId = null, int? selectedHallId = null)
        {
            var movies = await _context.Movies.ToListAsync();
            var halls = await _context.Halls.Include(h => h.Cinema).ToListAsync();

            var hallList = halls.Select(h => new
            {
                Id = h.Id,
                DisplayName = $"{h.Cinema?.Name} - {h.Name} (Cap: {h.Capacity})"
            }).ToList();

            ViewBag.Movies = new SelectList(movies, "Id", "Title", selectedMovieId);
            ViewBag.Halls = new SelectList(hallList, "Id", "DisplayName", selectedHallId);
        }

        private async Task<bool> ShowtimeExists(int id)
        {
            return await _context.Showtimes.AnyAsync(e => e.Id == id);
        }
    }
}
