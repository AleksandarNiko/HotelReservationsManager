using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationsManager.Data;
using HotelReservationsManager.Data.Models;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly HotelReservationsManagerDbContext _context;

        public UsersController(HotelReservationsManagerDbContext context) => _context = context;

        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var allowedSizes = new[] { 10, 25, 50 };
            if (!allowedSizes.Contains(pageSize)) pageSize = 10;

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Username.Contains(search) ||
                                         u.FirstName.Contains(search) ||
                                         u.MiddleName.Contains(search) ||
                                         u.LastName.Contains(search) ||
                                         u.Email.Contains(search));
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageSizeOptions = allowedSizes;
            ViewBag.Search = search;

            return View(items);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                // Hash the password before saving
                user.Password = Controllers.AccountController.HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            // Don't send hashed password to the form — clear it so user must re-enter or leave blank
            ViewBag.OriginalPasswordHash = user.Password;
            user.Password = string.Empty;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user, string originalPasswordHash)
        {
            // Remove password from model validation if left blank (keep existing hash)
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.Remove(nameof(Data.Models.User.Password));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OriginalPasswordHash = originalPasswordHash;
                return View(user);
            }

            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null) return NotFound();

            existing.Username = user.Username;
            existing.FirstName = user.FirstName;
            existing.MiddleName = user.MiddleName;
            existing.LastName = user.LastName;
            existing.EGN = user.EGN;
            existing.PhoneNumber = user.PhoneNumber;
            existing.Email = user.Email;
            existing.HireDate = user.HireDate;
            existing.IsActive = user.IsActive;
            existing.IsAdmin = user.IsAdmin;
            existing.LeavingDate = user.LeavingDate;

            // Only update password if a new one was provided
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.Password = Controllers.AccountController.HashPassword(user.Password);
            }
            else
            {
                existing.Password = originalPasswordHash;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                user.LeavingDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
