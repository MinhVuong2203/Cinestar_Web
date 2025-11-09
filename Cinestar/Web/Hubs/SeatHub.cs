using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs
{
    public class SeatHub : Hub
    {
        // Khi khách hàng join vào một suất chiếu
        public async Task JoinShowTime(string showTimeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, showTimeId);
        }

        // Khi khách hàng rời khỏi suất chiếu
        public async Task LeaveShowTime(string showTimeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, showTimeId);
        }


    }
}
