using HotelReservationsManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace HotelReservationsManager.Data
{
    public class HotelReservationsManagerDbContext : DbContext
    {
        public HotelReservationsManagerDbContext(DbContextOptions<HotelReservationsManagerDbContext> options)
            : base(options)
        {
        }

        public  DbSet<User> Users { get; set; }

        public  DbSet<Client> Clients { get; set; }

        public  DbSet<Room> Rooms { get; set; }

        public  DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EGN)
                .IsUnique();

            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.Clients)
                .WithMany(c => c.Reservations);

            base.OnModelCreating(modelBuilder);
        }
    }
}
