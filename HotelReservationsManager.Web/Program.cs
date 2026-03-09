using HotelReservationsManager.Data;
using HotelReservationsManager.Data.Models;
using HotelReservationsManager.Services.Interfaces;
using HotelReservationsManager.Services.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelReservationsManagerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<HotelReservationsManager.Web.Services.RoomCleanupService>();

var app = builder.Build();

// Seed Admin Logic
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<HotelReservationsManagerDbContext>();
        // Ensure database is created and migrations are applied at startup
        context.Database.Migrate();

        if (!context.Users.Any(u => u.Username == "admin"))
        {
            context.Users.Add(new User
            {
                Username = "admin",
                Password = "adminpassword", // В реална среда ползвай Hashing!
                FirstName = "Admin",
                MiddleName = "Admin",
                LastName = "Admin",
                EGN = "0000000000",
                PhoneNumber = "0000000000",
                Email = "admin@hotel.com",
                HireDate = DateTime.Now,
                IsActive = true
            });
            context.SaveChanges();
        }

        // Seed sample rooms if none
        if (!context.Rooms.Any())
        {
            context.Rooms.AddRange(
                new Room { Number = 101, Capacity = 2, Type = RoomType.DoubleBed, BedPriceAdult = 60, BedPriceYoung = 30, IsFree = true },
                new Room { Number = 102, Capacity = 2, Type = RoomType.TwoSingleBeds, BedPriceAdult = 55, BedPriceYoung = 25, IsFree = true },
                new Room { Number = 201, Capacity = 4, Type = RoomType.Apartment, BedPriceAdult = 120, BedPriceYoung = 60, IsFree = true }
            );
            context.SaveChanges();
        }

        // Seed sample clients if none
        if (!context.Clients.Any())
        {
            context.Clients.AddRange(
                new Client { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@example.com", PhoneNumber = "0888123456", IsAdult = true },
                new Client { FirstName = "Maria", LastName = "Petrova", Email = "maria@example.com", PhoneNumber = "0899123456", IsAdult = true }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Log the error so you can see why the web server stopped during startup
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<HotelReservationsManager.Web.Middlewares.ActiveUserMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
