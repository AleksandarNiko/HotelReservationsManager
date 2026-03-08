using HotelReservationsManager.Data.Models;
using HotelReservationsManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservationsManager.Services.Interfaces
{
    public interface IReservationService
    {
        Task<decimal> CalculatePrice(int roomId, List<int> clientIds, DateTime start, DateTime end, bool breakfast, bool allInc);
        Task<bool> CreateReservationAsync(ReservationCreateViewModel model, int userId);
        Task<IEnumerable<Reservation>> GetAllPagedAsync(string search, int page, int pageSize);
    }
}
