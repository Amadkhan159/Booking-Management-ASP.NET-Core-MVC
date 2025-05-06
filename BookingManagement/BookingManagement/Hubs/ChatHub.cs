using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BookingManagement.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int senderId, int receiverId, string message, string serviceName)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            await Clients.All.SendAsync("ReceiveMessage", senderId, receiverId, message, time, serviceName);
        }
    }
}
