using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HotelReservationsManager.ViewModels;
using HotelReservationsManager.Data;
using Microsoft.EntityFrameworkCore;


namespace HotelReservationsManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly HotelReservationsManagerDbContext _context;
        public HomeController(HotelReservationsManagerDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.RoomsCount = await _context.Rooms.CountAsync();
            ViewBag.FreeRooms = await _context.Rooms.CountAsync(r => r.IsFree);
            ViewBag.ClientsCount = await _context.Clients.CountAsync();
            ViewBag.ReservationsCount = await _context.Reservations.CountAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
