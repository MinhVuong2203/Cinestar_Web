using Web.Models;


namespace Web.Areas.Admin.Service
{
    public class MoviePagedResult
    {
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public string? SearchKeyword { get; set; }
        public string? FilterStatus { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetAllMoviesAsync();
        Task<IEnumerable<Movie>> GetActiveMoviesAsync();
        Task<Movie?> GetMovieByIdAsync(string movieId);
        Task<bool> CreateMovieAsync(Movie movie);
        Task<bool> UpdateMovieAsync(Movie movie);
        Task<bool> DeleteMovieAsync(string movieId);
        Task<bool> SoftDeleteMovieAsync(string movieId);
        Task<bool> RestoreMovieAsync(string movieId);
        Task<bool> MovieExistsAsync(string movieId);
        Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm);
        Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre);
        Task<IEnumerable<Movie>> GetCurrentShowingMoviesAsync();
        Task<IEnumerable<Movie>> GetUpcomingMoviesAsync();
        Task<MoviePagedResult> GetMoviesPagedAsync(int pageNumber, int pageSize, string? searchKeyword, string? filterStatus);

    }
}
