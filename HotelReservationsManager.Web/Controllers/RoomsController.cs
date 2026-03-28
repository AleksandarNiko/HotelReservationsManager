using HotelReservationsManager.Data;
using HotelReservationsManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly HotelReservationsManagerDbContext _context;

        public RoomsController(HotelReservationsManagerDbContext context) => _context = context;

        // Всички могат да разглеждат стаите (филтрирано и страницирано)
        public async Task<IActionResult> Index(string type, bool? isFree, int? capacity, int page = 1, int pageSize = 10)
        {
            var allowedSizes = new[] { 10, 25, 50 };
            if (!allowedSizes.Contains(pageSize)) pageSize = 10;

            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(r => r.Type.ToString() == type);

            if (isFree.HasValue)
                query = query.Where(r => r.IsFree == isFree.Value);

            if (capacity.HasValue)
                query = query.Where(r => r.Capacity == capacity.Value);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageSizeOptions = allowedSizes;

            return View(items);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Room room)
        {
            if (!ModelState.IsValid) return View(room);
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();
            return View(room);
        }
    }
}
