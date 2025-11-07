using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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

        // Phân trang với stored procedure
        public async Task<MoviePagedResult> GetMoviesPagedAsync(int pageNumber, int pageSize, string? searchKeyword, string? filterStatus)
        {
            var result = new MoviePagedResult
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                SearchKeyword = searchKeyword,
                FilterStatus = filterStatus
            };

            // Xác định giá trị IsCurrentlyShowing dựa trên filterStatus
            object isCurrentlyShowing = DBNull.Value;
            if (filterStatus == "showing")
            {
                isCurrentlyShowing = 1;
            }
            else if (filterStatus == "upcoming")
            {
                isCurrentlyShowing = 0;
            }
            else if (filterStatus == "ended")
            {
                isCurrentlyShowing = -1; // Sử dụng -1 để đánh dấu phim đã chiếu
            }
            // filterStatus == "all" hoặc null thì để DBNull.Value

            var pageNumberParam = new SqlParameter("@PageNumber", pageNumber);
            var pageSizeParam = new SqlParameter("@PageSize", pageSize);
            var searchKeywordParam = new SqlParameter("@SearchKeyword",
                string.IsNullOrWhiteSpace(searchKeyword) ? DBNull.Value : searchKeyword);
            var isCurrentlyShowingParam = new SqlParameter("@IsCurrentlyShowing", isCurrentlyShowing);

            var movies = await _context.Movies
                .FromSqlRaw("EXEC sp_GetMoviesPaged @PageNumber, @PageSize, @SearchKeyword, @IsCurrentlyShowing",
                    pageNumberParam, pageSizeParam, searchKeywordParam, isCurrentlyShowingParam)
                .ToListAsync();

            if (movies.Any())
            {
                result.Movies = movies;
                // TotalRecords và TotalPages được stored procedure trả về trong mỗi row
                // Lấy từ movie đầu tiên
                var firstMovie = movies.First();

                // Nếu bạn có cột TotalRecords trong kết quả, cần thêm vào Movie model
                // Hoặc dùng query riêng để lấy tổng số
                // Tạm thời tính lại total
                var totalCount = await GetTotalMoviesCountAsync(searchKeyword, filterStatus);
                result.TotalRecords = totalCount;
                result.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            }

            return result;
        }

        private async Task<int> GetTotalMoviesCountAsync(string? searchKeyword, string? filterStatus)
        {
            var query = _context.Movies.Where(m => !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                query = query.Where(m => m.Title.Contains(searchKeyword));
            }

            var now = DateTime.Now;
            if (filterStatus == "showing")
            {
                query = query.Where(m => m.StartTime <= now && (m.EndTime == null || m.EndTime >= now));
            }
            else if (filterStatus == "upcoming")
            {
                query = query.Where(m => m.StartTime > now);
            }
            else if (filterStatus == "ended")
            {
                query = query.Where(m => m.EndTime < now);
            }

            return await query.CountAsync();
        }
    }
}