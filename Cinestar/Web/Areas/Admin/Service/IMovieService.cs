using Web.Models;


namespace Web.Areas.Admin.Service
{
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
    }
}
