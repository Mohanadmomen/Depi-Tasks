using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CinemasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cinemas = await _context.Cinemas.ToListAsync();
            return View(cinemas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cinema cinema)
        {
            if (ModelState.IsValid)
            {
                await _context.Cinemas.AddAsync(cinema);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cinema '{cinema.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Failed to create cinema. Please fix errors.";
            return View(cinema);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                TempData["ErrorMessage"] = "Cinema not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cinema cinema)
        {
            if (id != cinema.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cinema);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cinema '{cinema.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CinemaExists(cinema.Id))
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
            TempData["ErrorMessage"] = "Failed to update cinema. Please fix errors.";
            return View(cinema);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _context.Cinemas
                .Include(c => c.Halls)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null)
            {
                TempData["ErrorMessage"] = "Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(cinema);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema != null)
            {
                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cinema '{cinema.Name}' deleted successfully along with all its halls.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CinemaExists(int id)
        {
            return await _context.Cinemas.AnyAsync(e => e.Id == id);
        }
    }
}
