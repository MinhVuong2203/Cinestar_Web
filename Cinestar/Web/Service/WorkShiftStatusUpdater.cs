using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Web.Data;

namespace Web.Service
{
    public class WorkShiftStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkShiftStatusUpdater> _logger;

        public WorkShiftStatusUpdater(IServiceProvider serviceProvider, ILogger<WorkShiftStatusUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("<---- WorkShiftStatusUpdater started ----->");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<CineStarContext>();

                        var now = DateTime.Now;
                        var shifts = await db.WorkShifts.ToListAsync(stoppingToken);

                        foreach (var shift in shifts)
                        {
                            if (shift.Status == "Hoàn thành" || shift.Status == "Vắng" || shift.Status == "Nghỉ phép")
                                continue;

                            if (now < shift.StartTime)
                                shift.Status = "Sắp làm";
                            else if (now >= shift.StartTime && now <= shift.EndTime)
                                shift.Status = "Đang làm";
                            else
                                shift.Status = "Hoàn thành";
                        }

                        await db.SaveChangesAsync(stoppingToken);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Lỗi khi cập nhật trạng thái ca làm việc.");
                }
            }

            _logger.LogInformation("<----- WorkShiftStatusUpdater stopped ----->");
        }
    }
}
