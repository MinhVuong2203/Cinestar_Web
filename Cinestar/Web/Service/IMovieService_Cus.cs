
using Web.Models;
namespace Web.Service
{
    public interface IMovieService_Cus
    {
        Task<List<Movie>> GetNowShowingMoviesAsync(int pageSize = 12);
        Task<List<Movie>> GetComingSoonMoviesAsync(int pageSize = 12);
        Task<Movie?> GetMovieByIdAsync(string movieId);
        // Lấy danh sách ngày chiếu của phim theo movieId
        List<string> GetMovieDates(string movieId);

        // Lấy danh sách giờ chiếu của phim theo movieId và date
        List<string> GetMovieShowTimes(string movieId, string date);
        public Task<Object> GetSeatingLayoutAsync(string showTimeId);
    }
}
