using HotelReservationsManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservationsManager.Services.Services
{
    public class RoomCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public RoomCleanupService(IServiceProvider services) => _services = services;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<HotelReservationsManagerDbContext>();

                    var expiredReservations = await context.Reservations
                        .Where(r => r.DepartureDate <= DateTime.Today)
                        .Select(r => r.RoomId)
                        .ToListAsync();

                    var roomsToFree = await context.Rooms
                        .Where(r => expiredReservations.Contains(r.Id))
                        .ToListAsync();

                    foreach (var room in roomsToFree) room.IsFree = true;

                    await context.SaveChangesAsync();
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Изпълнява се веднъж на ден
            }
        }
    }
}
