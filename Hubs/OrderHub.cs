using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using System.Security.Claims;

namespace SaleStore.Hubs
{
    [Authorize]
    public class OrderHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public OrderHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task JoinOrderGroup(string orderId)
        {
            var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdValue, out var userId) || !long.TryParse(orderId, out var parsedOrderId))
                throw new HubException("Order access denied.");

            var canAccess = Context.User?.IsInRole(AppRoles.Admin) == true ||
                Context.User?.IsInRole(AppRoles.Staff) == true ||
                await _db.Orders.AsNoTracking().AnyAsync(x => x.Id == parsedOrderId && x.CreatedByUserId == userId);

            if (!canAccess)
                throw new HubException("Order access denied.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{parsedOrderId}");
        }

        public async Task JoinAdminDashboard()
        {
            if (Context.User?.IsInRole(AppRoles.Admin) != true)
                throw new HubException("Admin access required.");

            await Groups.AddToGroupAsync(Context.ConnectionId, "admin-dashboard");
        }
    }
}
