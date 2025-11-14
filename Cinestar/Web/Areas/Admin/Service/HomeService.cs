using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Areas.Admin.Service
{
    public class HomeService : IHomeService
    {
        private readonly CineStarContext _context;

        public HomeService(CineStarContext context)
        {
            _context = context;
        }

        public int GetTotalMovies()
        {
            return _context.Movies
                .Where(m => !m.IsDeleted)
                .Count();
        }

        public int GetNowShowingMovies()
        {
            var now = DateTime.Now;
            return _context.Movies
                .Where(m => !m.IsDeleted &&
                           m.StartTime.HasValue && m.EndTime.HasValue &&
                           m.StartTime.Value <= now &&
                           m.EndTime.Value >= now)
                .Count();
        }

        public int GetComingSoon()
        {
            var now = DateTime.Now;
            return _context.Movies
                .Where(m => !m.IsDeleted &&
                           m.StartTime.HasValue &&
                           m.StartTime.Value > now)
                .Count();
        }

        public double GetAverageDuration()
        {
            var avgDuration = _context.Movies
                .Where(m => !m.IsDeleted &&
                           m.DurationMinutes.HasValue &&
                           m.DurationMinutes > 0)
                .Average(m => (double?)m.DurationMinutes);

            return avgDuration.HasValue ? Math.Round(avgDuration.Value, 0) : 0;
        }

        // HomeService.cs

        private const string INVOICE_STATUS_PAID = "Đã thanh toán"; // DÙNG CHUẨN TIẾNG VIỆT

        public async Task<decimal> GetMonthlyRevenue(DateTime from, DateTime to)
        {
            return await _context.Invoices
                .Where(i => !i.IsDeleted
                            && i.Status == INVOICE_STATUS_PAID  // CHỈ LẤY HÓA ĐƠN ĐÃ THANH TOÁN
                            && i.IssueDate >= from
                            && i.IssueDate <= to)
                .SumAsync(i => (decimal?)i.TotalAmount ?? 0m);
        }

        public async Task<double> GetRevenueGrowthPercentage(DateTime currentFrom, DateTime currentTo)
        {
            var currentRevenue = await GetMonthlyRevenue(currentFrom, currentTo);
            var prevFrom = currentFrom.AddMonths(-1);
            var prevTo = currentFrom.AddSeconds(-1);
            var prevRevenue = await GetMonthlyRevenue(prevFrom, prevTo);

            if (prevRevenue == 0) return 100; // tăng 100% nếu tháng trước = 0

            var growth = ((double)(currentRevenue - prevRevenue) / (double)prevRevenue) * 100;
            return Math.Round(growth, 1);
        }
    }
}