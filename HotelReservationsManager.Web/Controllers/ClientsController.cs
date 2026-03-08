using HotelReservationsManager.Data;
using HotelReservationsManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly HotelReservationsManagerDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(HotelReservationsManagerDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var allowedSizes = new[] { 10, 25, 50 };
            if (!allowedSizes.Contains(pageSize)) pageSize = 10;

            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.FirstName.Contains(search) || c.LastName.Contains(search) || c.Email.Contains(search));

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageSizeOptions = allowedSizes;

            return View(items);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return Problem("An error occurred while creating the client.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            try
            {
                if (!ModelState.IsValid) return View(client);
                _context.Clients.Update(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing client {ClientId}", client.Id);
                return Problem("An error occurred while editing the client.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await _context.Clients.FindAsync(id);
                if (client != null)
                {
                    _context.Clients.Remove(client);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                return Problem("An error occurred while deleting the client.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int page = 1, int pageSize = 10)
        {
            try
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client == null) return NotFound();

                var query = _context.Reservations
                    .Where(r => r.Clients.Any(c => c.Id == id))
                    .Include(r => r.Room)
                    .OrderByDescending(r => r.ArrivalDate)
                    .AsQueryable();

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                ViewBag.Client = client;
                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client details {ClientId}", id);
                return Problem("An error occurred while loading client details.");
            }
        }
    }
}
