using System.Security.Claims;
using HotelReservationsManager.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Web.Middlewares
{
    public class ActiveUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public ActiveUserMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip checks for non-authenticated users and static files / account endpoints
            var path = context.Request.Path.Value ?? string.Empty;
            if (context.User?.Identity?.IsAuthenticated == true
                && !path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("/images", StringComparison.OrdinalIgnoreCase))
            {
                var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out var userId))
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<HotelReservationsManagerDbContext>();
                        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                        if (user == null || !user.IsActive)
                        {
                            // sign out and redirect to login
                            await context.SignOutAsync();
                            context.Response.Redirect("/Account/Login");
                            return;
                        }
                    }
                    catch
                    {
                        // if DB fails, let the request continue; logging could be added
                    }
                }
            }

            await _next(context);
        }
    }
}
