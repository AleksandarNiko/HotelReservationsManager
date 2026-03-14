using System;
using System.Threading;
using System.Threading.Tasks;
using HotelReservationsManager.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Web.Services
{
    public class RoomCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public RoomCleanupService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<HotelReservationsManagerDbContext>();

                    var now = DateTime.Now;
                    // Find reservations that have passed departure date and mark their rooms free
                    var expired = await db.Reservations
                        .Include(r => r.Room)
                        .Where(r => r.DepartureDate <= now && r.Room != null && !r.Room.IsFree)
                        .ToListAsync(stoppingToken);

                    foreach (var res in expired)
                    {
                        res.Room.IsFree = true;
                    }

                    if (expired.Count > 0)
                    {
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch
                {
                    // swallow - avoid crashing the background service. In a real app, log.
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
