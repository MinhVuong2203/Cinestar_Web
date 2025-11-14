using Web.Models;

namespace Web.Areas.Admin.Service
{
    // Pagination Models
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class ShowTimeDto
    {
        public string ShowTimeID { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public decimal Price { get; set; }
        public string MovieID { get; set; } = null!;
        public string MovieTitle { get; set; } = null!;
        public string RoomID { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string BranchID { get; set; } = null!;
        public string BranchName { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int DurationMinutes { get; set; }
    }

    // Interface
    public interface IShowTimeService
    {
        // Basic CRUD
        Task<List<ShowTime>> GetAllAsync();
        Task<ShowTime?> GetByIdAsync(string id);
        Task<bool> CreateAsync(ShowTime showTime);
        Task<bool> UpdateAsync(ShowTime showTime);
        Task<bool> SoftDeleteAsync(string id);
        Task<bool> RestoreAsync(string id);

        // Lookup methods
        Task<List<Movie>> GetAllMoviesAsync();
        Task<List<CinemaBranch>> GetAllBranchesAsync();
        Task<List<Room>> GetRoomsByBranchAsync(string branchId);
        Task<Movie?> GetMovieByIdAsync(string movieId);
        Task<CinemaBranch?> GetBranchByIdAsync(string branchId);
        Task<Room?> GetRoomByIdAsync(string roomId); // ✅ NEW

        // Timeline & Conflict checking
        Task<List<ShowTime>> GetShowTimesByRoomAndDateAsync(string roomId, DateTime date);
        Task<bool> CheckTimeConflictAsync(string roomId, DateTime startTime, DateTime endTime, string? excludeShowTimeId = null);
        Task<List<ShowTime>> GetConflictingShowTimesAsync(string roomId, DateTime startTime, DateTime endTime, string? excludeShowTimeId = null);

        // Pagination
        Task<PagedResult<ShowTimeDto>> GetShowTimesPagedAsync(int pageNumber = 1, int pageSize = 10, string? branchId = null, string? movieId = null, string? roomId = null);
    }
}