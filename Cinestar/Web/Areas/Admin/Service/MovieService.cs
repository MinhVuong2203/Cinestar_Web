using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class MovieService : IMovieService
    {
        private readonly CineStarContext _context;

        public MovieService(CineStarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            return await _context.Movies
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> GetActiveMoviesAsync()
        {
            return await _context.Movies
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();
        }

        public async Task<Movie?> GetMovieByIdAsync(string movieId)
        {
            return await _context.Movies
                .Include(m => m.ShowTimes)
                .Include(m => m.MovieProducts)
                .FirstOrDefaultAsync(m => m.MovieID == movieId);
        }

        public async Task<bool> CreateMovieAsync(Movie movie)
        {
            try
            {
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateMovieAsync(Movie movie)
        {
            try
            {
                _context.Entry(movie).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMovieAsync(string movieId)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(movieId);
                if (movie == null) return false;

                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeleteMovieAsync(string movieId)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(movieId);
                if (movie == null) return false;

                movie.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RestoreMovieAsync(string movieId)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(movieId);
                if (movie == null) return false;

                movie.IsDeleted = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MovieExistsAsync(string movieId)
        {
            return await _context.Movies.AnyAsync(m => m.MovieID == movieId);
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm)
        {
            return await _context.Movies
                .Where(m => !m.IsDeleted &&
                    (m.Title.Contains(searchTerm) ||
                     m.Genre!.Contains(searchTerm) ||
                     m.Description!.Contains(searchTerm)))
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre)
        {
            return await _context.Movies
                .Where(m => !m.IsDeleted && m.Genre == genre)
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> GetCurrentShowingMoviesAsync()
        {
            var today = DateTime.Now;
            return await _context.Movies
                .Where(m => !m.IsDeleted &&
                    m.StartTime <= today &&
                    m.EndTime >= today)
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> GetUpcomingMoviesAsync()
        {
            var today = DateTime.Now;
            return await _context.Movies
                .Where(m => !m.IsDeleted && m.StartTime > today)
                .OrderBy(m => m.StartTime)
                .ToListAsync();
        }
    }
}
