using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Hubs;

namespace Web.Service
{
    public class SeatCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        public SeatCleanupBackgroundService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CineStarContext>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SeatHub>>();
                // Tìm và giải phóng ghế hết hạn
                var expiredTickets = await context.Tickets
                    .Where(t =>
                        t.Status == "Đang được chọn" &&
                        t.LockedAt.HasValue &&
                        EF.Functions.DateDiffMinute(t.LockedAt.Value, DateTime.Now) >= 5)
                    .ToListAsync(stoppingToken);
                foreach (var ticket in expiredTickets)
                {
                    ticket.Status = "Trống";
                    ticket.LockedBy = null;
                    ticket.LockedAt = null;
                    await hubContext.Clients.Group(ticket.ShowTimeID).SendAsync("SeatDeselected", new
                    {
                        seatId = ticket.SeatID,
                        status = "Trống"
                    }, stoppingToken);
                }
                await context.SaveChangesAsync(stoppingToken);
                // Chờ 1 phút rồi chạy lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
