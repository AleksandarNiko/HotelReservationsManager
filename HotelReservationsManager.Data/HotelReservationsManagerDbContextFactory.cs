using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
// no Microsoft.Extensions.Configuration to keep design-time factory dependency-free

namespace HotelReservationsManager.Data
{
    // Design-time factory to allow EF Core tools to create the DbContext
    public class HotelReservationsManagerDbContextFactory : IDesignTimeDbContextFactory<HotelReservationsManagerDbContext>
    {
        public HotelReservationsManagerDbContext CreateDbContext(string[] args)
        {
            // Prefer an environment variable for design-time connection, fallback to localdb
            var connectionString = Environment.GetEnvironmentVariable("HOTELCONN")
                                   ?? "Server=(localdb)\\MSSQLLocalDB;Database=HotelReservationsManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            var optionsBuilder = new DbContextOptionsBuilder<HotelReservationsManagerDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new HotelReservationsManagerDbContext(optionsBuilder.Options);
        }
    }
}
