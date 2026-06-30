using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HallsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HallsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var halls = await _context.Halls
                .Include(h => h.Cinema)
                .ToListAsync();
            return View(halls);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Cinemas = new SelectList(await _context.Cinemas.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hall hall)
        {
            if (ModelState.IsValid)
            {
                await _context.Halls.AddAsync(hall);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Hall '{hall.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Cinemas = new SelectList(await _context.Cinemas.ToListAsync(), "Id", "Name", hall.CinemaId);
            TempData["ErrorMessage"] = "Failed to create hall. Please fix errors.";
            return View(hall);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
            {
                TempData["ErrorMessage"] = "Hall not found.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Cinemas = new SelectList(await _context.Cinemas.ToListAsync(), "Id", "Name", hall.CinemaId);
            return View(hall);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hall hall)
        {
            if (id != hall.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hall);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Hall '{hall.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await HallExists(hall.Id))
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
            ViewBag.Cinemas = new SelectList(await _context.Cinemas.ToListAsync(), "Id", "Name", hall.CinemaId);
            TempData["ErrorMessage"] = "Failed to update hall. Please fix errors.";
            return View(hall);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var hall = await _context.Halls
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hall == null)
            {
                TempData["ErrorMessage"] = "Hall not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(hall);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall != null)
            {
                _context.Halls.Remove(hall);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Hall '{hall.Name}' deleted successfully along with all its showtimes.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> HallExists(int id)
        {
            return await _context.Halls.AnyAsync(e => e.Id == id);
        }
    }
}
