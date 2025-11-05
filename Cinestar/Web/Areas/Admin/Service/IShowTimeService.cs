using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface IShowTimeService
    {
        Task<List<ShowTime>> GetAllAsync();
        Task<ShowTime?> GetByIdAsync(string id);
        Task<bool> CreateAsync(ShowTime showTime);
        Task<bool> UpdateAsync(ShowTime showTime);
        Task<bool> SoftDeleteAsync(string id);
        Task<bool> RestoreAsync(string id);

        // Thêm các method mới
        Task<List<Movie>> GetAllMoviesAsync();
        Task<List<CinemaBranch>> GetAllBranchesAsync();
        Task<List<Room>> GetRoomsByBranchAsync(string branchId);
        Task<Movie?> GetMovieByIdAsync(string movieId);
        Task<List<ShowTime>> GetShowTimesByRoomAndDateAsync(string roomId, DateTime date);
        Task<bool> CheckTimeConflictAsync(string roomId, DateTime startTime, DateTime endTime, string? excludeShowTimeId = null);

        Task<List<ShowTime>> GetConflictingShowTimesAsync(string roomId, DateTime startTime, DateTime endTime, string? excludeShowTimeId = null);
    }
}