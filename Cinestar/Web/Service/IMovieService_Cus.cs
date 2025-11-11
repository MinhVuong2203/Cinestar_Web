
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
        public Task<Object> GetSeatingLayoutAsync(string showTimeId, Guid currentCustomerId);
        public Task<bool> TrySelectSeatAsync(string showTimeId, string seatId, Guid guid);
        public Task<bool> DeselectSeatAsync(string showTimeId, string seatId, Guid guid);

        //lấy vé theo ghế ID, showtimeID
        public Task<Ticket?> GetTicketBySeatIdAsync(string showTimeId, string seatId);
    }
}
