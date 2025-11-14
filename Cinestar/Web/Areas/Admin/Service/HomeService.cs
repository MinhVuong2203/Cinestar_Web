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
    }
}