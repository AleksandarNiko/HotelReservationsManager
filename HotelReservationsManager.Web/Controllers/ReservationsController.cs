using HotelReservationsManager.Data;
using HotelReservationsManager.Services.Interfaces;
using HotelReservationsManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _resService;
        private readonly HotelReservationsManagerDbContext _context;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(IReservationService resService, HotelReservationsManagerDbContext context, ILogger<ReservationsController> logger)
        {
            _resService = resService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            try
            {
                var allowedSizes = new[] { 10, 25, 50 };
                if (!allowedSizes.Contains(pageSize)) pageSize = 10;

                var query = _context.Reservations
                    .Include(r => r.Room)
                    .Include(r => r.Clients)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r => r.Clients.Any(c => c.LastName.Contains(search) || c.FirstName.Contains(search)));
                }

                var total = await query.CountAsync();
                var items = await query.OrderByDescending(r => r.ArrivalDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.PageSizeOptions = allowedSizes;
                ViewBag.Search = search;

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservations index");
                return Problem("An error occurred while loading reservations.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Rooms = await _context.Rooms.Where(r => r.IsFree).ToListAsync();
            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var kv in ModelState)
                        foreach (var err in kv.Value.Errors)
                            _logger.LogWarning("ModelState error for {Key}: {Error}", kv.Key, err.ErrorMessage);
                }
                if (ModelState.IsValid)
                {
                    var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(idClaim))
                    {
                        _logger.LogWarning("Unable to find NameIdentifier claim.");
                        return Challenge();
                    }
                    if (!int.TryParse(idClaim, out var userId))
                    {
                        _logger.LogWarning("Invalid NameIdentifier: {Value}", idClaim);
                        return Problem("User identity is invalid.");
                    }

                    var success = await _resService.CreateReservationAsync(model, userId);
                    if (success) return RedirectToAction(nameof(Index));

                    ModelState.AddModelError("", "Стаята вече не е свободна.");
                }
                ViewBag.Rooms = await _context.Rooms.Where(r => r.IsFree).ToListAsync();
                ViewBag.Clients = await _context.Clients.ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return Problem("An error occurred while creating reservation.");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var res = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Clients)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (res == null) return NotFound();
            return View(res);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _context.Reservations
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return NotFound();

            var model = new ReservationCreateViewModel
            {
                RoomId = res.RoomId,
                SelectedClientIds = res.Clients.Select(c => c.Id).ToList(),
                CheckInDate = res.ArrivalDate,
                CheckOutDate = res.DepartureDate,
                HasBreakfast = res.HasBreakfast,
                IsAllInclusive = res.IsAllInclusive
            };

            ViewBag.Rooms = await _context.Rooms.Where(r => r.IsFree || r.Id == res.RoomId).ToListAsync();
            ViewBag.Clients = await _context.Clients.ToListAsync();
            ViewBag.ReservationId = id;
            ViewBag.CurrentSum = res.Sum;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReservationCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = await _context.Rooms.Where(r => r.IsFree || r.Id == model.RoomId).ToListAsync();
                ViewBag.Clients = await _context.Clients.ToListAsync();
                ViewBag.ReservationId = id;
                return View(model);
            }

            var reservation = await _context.Reservations
                .Include(r => r.Clients)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();

            if (reservation.RoomId != model.RoomId)
            {
                var newRoom = await _context.Rooms.FindAsync(model.RoomId);
                if (newRoom == null || !newRoom.IsFree)
                {
                    ModelState.AddModelError("", "Избраната стая не е свободна.");
                    ViewBag.Rooms = await _context.Rooms.Where(r => r.IsFree || r.Id == reservation.RoomId).ToListAsync();
                    ViewBag.Clients = await _context.Clients.ToListAsync();
                    ViewBag.ReservationId = id;
                    return View(model);
                }
                if (reservation.Room != null) reservation.Room.IsFree = true;
                newRoom.IsFree = false;
                reservation.RoomId = model.RoomId;
            }

            reservation.ArrivalDate = model.CheckInDate;
            reservation.DepartureDate = model.CheckOutDate;
            reservation.HasBreakfast = model.HasBreakfast;
            reservation.IsAllInclusive = model.IsAllInclusive;

            reservation.Clients.Clear();
            var clients = await _context.Clients.Where(c => model.SelectedClientIds.Contains(c.Id)).ToListAsync();
            foreach (var c in clients) reservation.Clients.Add(c);

            reservation.Sum = await _resService.CalculatePrice(
                reservation.RoomId,
                reservation.Clients.Select(c => c.Id).ToList(),
                reservation.ArrivalDate,
                reservation.DepartureDate,
                reservation.HasBreakfast,
                reservation.IsAllInclusive);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return NotFound();
            return View(res);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return NotFound();

            if (res.Room != null) res.Room.IsFree = true;
            _context.Reservations.Remove(res);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // AJAX endpoint for live price calculation
        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] PriceCalculationRequest req)
        {
            if (req == null || req.RoomId <= 0 || req.ClientIds == null || !req.ClientIds.Any())
                return Json(new { sum = 0m });

            try
            {
                var sum = await _resService.CalculatePrice(
                    req.RoomId,
                    req.ClientIds,
                    req.CheckIn,
                    req.CheckOut,
                    req.HasBreakfast,
                    req.IsAllInclusive);
                return Json(new { sum });
            }
            catch
            {
                return Json(new { sum = 0m });
            }
        }
    }

    public class PriceCalculationRequest
    {
        public int RoomId { get; set; }
        public List<int> ClientIds { get; set; } = new();
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public bool HasBreakfast { get; set; }
        public bool IsAllInclusive { get; set; }
    }
}
