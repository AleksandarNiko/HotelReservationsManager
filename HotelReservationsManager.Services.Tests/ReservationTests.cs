using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelReservationsManager.Data;
using HotelReservationsManager.Data.Models;
using HotelReservationsManager.Services.Services;
using HotelReservationsManager.ViewModels;
using Xunit;

namespace HotelReservationsManager.Services.Tests
{
    public class ReservationTests
    {
        private HotelReservationsManagerDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<HotelReservationsManagerDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                .Options;
            return new HotelReservationsManagerDbContext(options);
        }

        [Fact]
        public async Task CalculatePrice_AdultNoExtras_ReturnsCorrectTotal()
        {
            using var context = CreateDb("test_adult_basic");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50 });
            context.Clients.Add(new Client { Id = 1, IsAdult = true });
            context.SaveChanges();

            var total = await service.CalculatePrice(1, new List<int> { 1 },
                DateTime.Today, DateTime.Today.AddDays(2), false, false);

            Assert.Equal(200m, total);
        }

        [Fact]
        public async Task CalculatePrice_ChildNoExtras_UsesYoungPrice()
        {
            using var context = CreateDb("test_child_basic");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50 });
            context.Clients.Add(new Client { Id = 1, IsAdult = false });
            context.SaveChanges();

            var total = await service.CalculatePrice(1, new List<int> { 1 },
                DateTime.Today, DateTime.Today.AddDays(2), false, false);

            Assert.Equal(100m, total);
        }

        [Fact]
        public async Task CalculatePrice_WithBreakfast_AddsPerPersonPerNight()
        {
            using var context = CreateDb("test_breakfast");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50 });
            context.Clients.Add(new Client { Id = 1, IsAdult = true });
            context.SaveChanges();

            // 2 nights, 1 adult: (100 + 15) * 2 = 230
            var total = await service.CalculatePrice(1, new List<int> { 1 },
                DateTime.Today, DateTime.Today.AddDays(2), true, false);

            Assert.Equal(230m, total);
        }

        [Fact]
        public async Task CalculatePrice_WithAllInclusive_AddsPerPersonPerNight()
        {
            using var context = CreateDb("test_allinclusive");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50 });
            context.Clients.Add(new Client { Id = 1, IsAdult = true });
            context.SaveChanges();

            // 2 nights, 1 adult: (100 + 40) * 2 = 280
            var total = await service.CalculatePrice(1, new List<int> { 1 },
                DateTime.Today, DateTime.Today.AddDays(2), false, true);

            Assert.Equal(280m, total);
        }

        [Fact]
        public async Task CalculatePrice_BreakfastAndAllInclusive_BothAdded()
        {
            using var context = CreateDb("test_both_extras");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50 });
            context.Clients.Add(new Client { Id = 1, IsAdult = true });
            context.SaveChanges();

            // 2 nights: (100 + 15 + 40) * 2 = 310
            var total = await service.CalculatePrice(1, new List<int> { 1 },
                DateTime.Today, DateTime.Today.AddDays(2), true, true);

            Assert.Equal(310m, total);
        }

        [Fact]
        public async Task CalculatePrice_MultipleClients_SumsAllClients()
        {
            using var context = CreateDb("test_multi_clients");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50 });
            context.Clients.AddRange(
                new Client { Id = 1, IsAdult = true },
                new Client { Id = 2, IsAdult = false }
            );
            context.SaveChanges();

            // 2 nights: adult = 100*2=200, child = 50*2=100 => total 300
            var total = await service.CalculatePrice(1, new List<int> { 1, 2 },
                DateTime.Today, DateTime.Today.AddDays(2), false, false);

            Assert.Equal(300m, total);
        }

        [Fact]
        public async Task CalculatePrice_SameDayDeparture_CountsAsOnNight()
        {
            using var context = CreateDb("test_same_day");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50 });
            context.Clients.Add(new Client { Id = 1, IsAdult = true });
            context.SaveChanges();

            // 0 nights clamped to 1: 100 * 1 = 100
            var total = await service.CalculatePrice(1, new List<int> { 1 },
                DateTime.Today, DateTime.Today, false, false);

            Assert.Equal(100m, total);
        }

        [Fact]
        public async Task CreateReservationAsync_FreeRoom_CreatesAndMarksOccupied()
        {
            using var context = CreateDb("test_create_reservation");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50, IsFree = true });
            context.Clients.Add(new Client { Id = 1, IsAdult = true });
            context.SaveChanges();

            var model = new ReservationCreateViewModel
            {
                RoomId = 1,
                SelectedClientIds = new List<int> { 1 },
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(2),
                HasBreakfast = false,
                IsAllInclusive = false
            };

            var result = await service.CreateReservationAsync(model, userId: 1);

            Assert.True(result);
            var room = await context.Rooms.FindAsync(1);
            Assert.False(room.IsFree);
        }

        [Fact]
        public async Task CreateReservationAsync_OccupiedRoom_ReturnsFalse()
        {
            using var context = CreateDb("test_create_occupied");
            var service = new ReservationService(context);

            context.Rooms.Add(new Room { Id = 1, BedPriceAdult = 100, BedPriceYoung = 50, IsFree = false });
            context.Clients.Add(new Client { Id = 1, IsAdult = true });
            context.SaveChanges();

            var model = new ReservationCreateViewModel
            {
                RoomId = 1,
                SelectedClientIds = new List<int> { 1 },
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1),
                HasBreakfast = false,
                IsAllInclusive = false
            };

            var result = await service.CreateReservationAsync(model, userId: 1);

            Assert.False(result);
        }
    }
}
