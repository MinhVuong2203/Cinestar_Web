using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web.Data;
using Web.Models;

namespace Web.Service
{
    public class MovieService_Cus : IMovieService_Cus
    {
        private readonly CineStarContext _context;

        public MovieService_Cus(CineStarContext context)
        {
            _context = context;
        }

        public async Task<List<Movie>> GetNowShowingMoviesAsync(int pageSize = 12)
        {
            var now = DateTime.Now;
            Debug.WriteLine($"=== GetNowShowingMoviesAsync ===");
            Debug.WriteLine($"Current DateTime: {now}");
            Debug.WriteLine($"Query: Phim đang chiếu - StartTime <= {now} AND EndTime >= {now}");

            var movies = await _context.Movies
                .Where(m => !m.IsDeleted
                    && m.StartTime.HasValue
                    && m.EndTime.HasValue
                    && m.StartTime.Value <= now  // Đã bắt đầu chiếu
                    && m.EndTime.Value >= now)   // Chưa kết thúc
                .OrderBy(m => m.StartTime)
                .Take(pageSize)
                .ToListAsync();

            Debug.WriteLine($"Found {movies.Count} movies");
            foreach (var movie in movies)
            {
                Debug.WriteLine($"  - {movie.MovieID}: {movie.Title}");
                Debug.WriteLine($"    StartTime: {movie.StartTime}, EndTime: {movie.EndTime}");
            }

            return movies;
        }

        public async Task<List<Movie>> GetComingSoonMoviesAsync(int pageSize = 12)
        {
            var now = DateTime.Now;
            Debug.WriteLine($"=== GetComingSoonMoviesAsync ===");
            Debug.WriteLine($"Current DateTime: {now}");
            Debug.WriteLine($"Query: Phim sắp chiếu - StartTime > {now}");

            var movies = await _context.Movies
                .Where(m => !m.IsDeleted
                    && m.StartTime.HasValue
                    && m.StartTime.Value > now)  // Chưa bắt đầu chiếu
                .OrderBy(m => m.StartTime)
                .Take(pageSize)
                .ToListAsync();

            Debug.WriteLine($"Found {movies.Count} movies");
            foreach (var movie in movies)
            {
                Debug.WriteLine($"  - {movie.MovieID}: {movie.Title}");
                Debug.WriteLine($"    StartTime: {movie.StartTime}, EndTime: {movie.EndTime}");
            }

            return movies;
        }

        public async Task<Movie?> GetMovieByIdAsync(string movieId)
        {
            var movie = await _context.Movies
                .Where(m => m.MovieID == movieId && !m.IsDeleted)
                .FirstOrDefaultAsync();

            return movie;
        }

        //lấy danh sách ngày chiếu phim dd/mm/yyyy
        public List<string> GetMovieDates(string movieId)
        {
            try
            {
                var dates = _context.ShowTimes
                    .Where(st => !st.IsDeleted
                        && st.MovieID == movieId
                        && st.StartTime >= DateTime.Today)
                    .Select(st => st.StartTime.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .Select(d => d.ToString("dd/MM/yyyy"))
                    .ToList();

                return dates;
            }
            catch
            {
                return new List<string>();
            }
        }

        //lấy danh sách giờ chiếu phim theo movieId và date
        public List<string> GetMovieShowTimes(string movieId, string date)
        {
            try
            {
                DateTime parsedDate = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                var showTimes = _context.ShowTimes
                    .Where(st => !st.IsDeleted
                        && st.MovieID == movieId
                        && st.StartTime.Date == parsedDate.Date
                        && st.StartTime >= DateTime.Now)
                    .Select(st => st.StartTime.ToString("HH:mm"))
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
                return showTimes;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<Object> GetSeatingLayoutAsync(string showTimeId)
        {
            var tickets = await _context.Tickets
                .Include(t => t.Seat)
                .Where(t => t.ShowTimeID == showTimeId && !t.IsDeleted)
                .Select(t => new
                {
                    seatName = t.Seat.SeatName,
                    seatType = t.Seat.SeatType,
                    status = t.Status
                }).OrderBy(t => t.seatName).ToListAsync();
            return tickets;
        }




    }
}