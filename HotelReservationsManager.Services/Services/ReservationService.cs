using HotelReservationsManager.Data.Models;
using HotelReservationsManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using HotelReservationsManager.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HotelReservationsManager.ViewModels;

namespace HotelReservationsManager.Services.Services
{
    public class ReservationService : IReservationService
    {
        private readonly HotelReservationsManagerDbContext _context;

        public ReservationService(HotelReservationsManagerDbContext context) => _context = context;

        public async Task<decimal> CalculatePrice(int roomId, List<int> clientIds, DateTime start, DateTime end, bool breakfast, bool allInc)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            var clients = await _context.Clients.Where(c => clientIds.Contains(c.Id)).ToListAsync();

            int nights = (end - start).Days;
            if (nights <= 0) nights = 1;

            decimal total = 0;
            foreach (var client in clients)
            {
                decimal nightPrice = client.IsAdult ? room.BedPriceAdult : room.BedPriceYoung;
                if (breakfast) nightPrice += 15; 
                if (allInc) nightPrice += 40;    
                total += nightPrice * nights;
            }
            return total;
        }

        public async Task<bool> CreateReservationAsync(ReservationCreateViewModel model, int userId)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null || !room.IsFree) return false;

            var reservation = new Reservation
            {
                RoomId = model.RoomId,
                UserId = userId,
                ArrivalDate = model.CheckInDate,
                DepartureDate = model.CheckOutDate,
                HasBreakfast = model.HasBreakfast,
                IsAllInclusive = model.IsAllInclusive,
                Sum = await CalculatePrice(model.RoomId, model.SelectedClientIds, model.CheckInDate, model.CheckOutDate, model.HasBreakfast, model.IsAllInclusive)
            };

            var clients = await _context.Clients.Where(c => model.SelectedClientIds.Contains(c.Id)).ToListAsync();
            foreach (var client in clients) reservation.Clients.Add(client);

            room.IsFree = false; 
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Reservation>> GetAllPagedAsync(string search, int page, int pageSize)
        {
            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Clients)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Clients.Any(c => c.LastName.Contains(search) || c.FirstName.Contains(search)));
            }

            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
    }
}
