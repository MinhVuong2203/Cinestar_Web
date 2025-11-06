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
    }
}