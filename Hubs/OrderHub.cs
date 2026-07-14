using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Hubs
{
    public class OrderHub : Hub
    {
        public async Task NotifyUpdate()
        {
            await Clients.All.SendAsync("OrderUpdated");
        }
    }
}
