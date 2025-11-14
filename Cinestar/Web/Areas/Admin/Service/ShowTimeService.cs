using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class ShowTimeService : IShowTimeService
    {
        private readonly CineStarContext _db;

        public ShowTimeService(CineStarContext db)
        {
            _db = db;
        }

        // NEW: Pagination method using stored procedure
        public async Task<PagedResult<ShowTimeDto>> GetShowTimesPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? branchId = null,
            string? movieId = null,
            string? roomId = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@PageNumber", pageNumber),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@BranchID", (object?)branchId ?? DBNull.Value),
                new SqlParameter("@MovieID", (object?)movieId ?? DBNull.Value),
                new SqlParameter("@RoomID", (object?)roomId ?? DBNull.Value)
            };

            var result = await _db.Database
                .SqlQueryRaw<ShowTimeDto>(
                    "EXEC sp_GetShowTimesPaged @PageNumber, @PageSize, @BranchID, @MovieID, @RoomID",
                    parameters)
                .ToListAsync();

            var pagedResult = new PagedResult<ShowTimeDto>
            {
                Items = result,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };

            if (result.Any())
            {
                pagedResult.TotalRecords = result.First().TotalRecords;
                pagedResult.TotalPages = result.First().TotalPages;
            }

            return pagedResult;
        }

        public async Task<List<ShowTime>> GetAllAsync()
            => await _db.ShowTimes
                .Include(x => x.Movie)
                .Include(x => x.Room)
                    .ThenInclude(r => r.Branch)
                .OrderByDescending(x => x.StartTime)
                .ToListAsync();

        public async Task<ShowTime?> GetByIdAsync(string id)
            => await _db.ShowTimes
                .Include(x => x.Movie)
                .Include(x => x.Room)
                    .ThenInclude(r => r.Branch)
                .FirstOrDefaultAsync(st => st.ShowTimeID == id);

        public async Task<bool> CreateAsync(ShowTime showTime)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("\n========== SERVICE CREATE ==========");
                System.Diagnostics.Debug.WriteLine($"ShowTimeID: '{showTime.ShowTimeID}'");
                System.Diagnostics.Debug.WriteLine($"MovieID: '{showTime.MovieID}'");
                System.Diagnostics.Debug.WriteLine($"RoomID: '{showTime.RoomID}'");
                System.Diagnostics.Debug.WriteLine($"StartTime: {showTime.StartTime}");
                System.Diagnostics.Debug.WriteLine($"Price: {showTime.Price}");
                System.Diagnostics.Debug.WriteLine($"IsDeleted: {showTime.IsDeleted}");

                showTime.IsDeleted = false;

                System.Diagnostics.Debug.WriteLine("\nAdding to context...");
                _db.ShowTimes.Add(showTime);

                System.Diagnostics.Debug.WriteLine("Calling SaveChangesAsync...");
                var saveResult = await _db.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"✅ SUCCESS! Rows affected: {saveResult}");
                System.Diagnostics.Debug.WriteLine("====================================\n");
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                System.Diagnostics.Debug.WriteLine("\n❌❌❌ DATABASE UPDATE EXCEPTION ❌❌❌");
                System.Diagnostics.Debug.WriteLine($"Message: {dbEx.Message}");

                if (dbEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"\nInner Exception: {dbEx.InnerException.Message}");

                    if (dbEx.InnerException.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"\nSQL Error Detail: {dbEx.InnerException.InnerException.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"\nStack Trace:\n{dbEx.StackTrace}");
                System.Diagnostics.Debug.WriteLine("============================================\n");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("\n❌❌❌ GENERAL EXCEPTION ❌❌❌");
                System.Diagnostics.Debug.WriteLine($"Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");

                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"\nInner: {ex.InnerException.Message}");

                System.Diagnostics.Debug.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine("======================================\n");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ShowTime showTime)
        {
            try
            {
                _db.ShowTimes.Update(showTime);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            var st = await _db.ShowTimes.FindAsync(id);
            if (st == null) return false;
            st.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(string id)
        {
            var st = await _db.ShowTimes.FindAsync(id);
            if (st == null) return false;
            st.IsDeleted = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<Movie>> GetAllMoviesAsync()
            => await _db.Movies.Where(m => !m.IsDeleted).OrderBy(m => m.Title).ToListAsync();

        public async Task<List<CinemaBranch>> GetAllBranchesAsync()
            => await _db.CinemaBranches.Where(b => !b.IsDeleted).OrderBy(b => b.BranchName).ToListAsync();

        public async Task<List<Room>> GetRoomsByBranchAsync(string branchId)
            => await _db.Rooms.Where(r => r.BranchID == branchId && !r.IsDeleted).OrderBy(r => r.RoomName).ToListAsync();

        // ✅ UPDATED: Include ReleaseDate and EndTime
        public async Task<Movie?> GetMovieByIdAsync(string movieId)
            => await _db.Movies
                .Where(m => m.MovieID == movieId)
                .FirstOrDefaultAsync();

        public async Task<CinemaBranch?> GetBranchByIdAsync(string branchId)
            => await _db.CinemaBranches.FindAsync(branchId);

        // ✅ NEW: Get Room with Branch info
        public async Task<Room?> GetRoomByIdAsync(string roomId)
            => await _db.Rooms
                .Include(r => r.Branch)
                .FirstOrDefaultAsync(r => r.RoomID == roomId);

        public async Task<List<ShowTime>> GetShowTimesByRoomAndDateAsync(string roomId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);
            return await _db.ShowTimes
                .Include(x => x.Movie)
                .Where(st => st.RoomID == roomId && !st.IsDeleted && st.StartTime >= startOfDay && st.StartTime < endOfDay)
                .OrderBy(st => st.StartTime)
                .ToListAsync();
        }

        public async Task<bool> CheckTimeConflictAsync(string roomId, DateTime startTime, DateTime endTime, string? excludeShowTimeId = null)
        {
            const int bufferMinutes = 15;
            var query = _db.ShowTimes.Include(st => st.Movie)
                .Where(st => st.RoomID == roomId && !st.IsDeleted && st.StartTime.Date == startTime.Date);

            if (!string.IsNullOrEmpty(excludeShowTimeId))
                query = query.Where(st => st.ShowTimeID != excludeShowTimeId);

            var existingShowTimes = await query.ToListAsync();

            foreach (var existing in existingShowTimes)
            {
                var existingEnd = existing.StartTime.AddMinutes(existing.Movie?.DurationMinutes ?? 0);
                var existingStartBuffer = existing.StartTime.AddMinutes(-bufferMinutes);
                var existingEndBuffer = existingEnd.AddMinutes(bufferMinutes);

                if ((startTime >= existingStartBuffer && startTime < existingEndBuffer) ||
                    (endTime > existingStartBuffer && endTime <= existingEndBuffer) ||
                    (startTime <= existingStartBuffer && endTime >= existingEndBuffer))
                    return true;
            }
            return false;
        }

        public async Task<List<ShowTime>> GetConflictingShowTimesAsync(string roomId, DateTime startTime, DateTime endTime, string? excludeShowTimeId = null)
        {
            const int bufferMinutes = 15;
            var query = _db.ShowTimes.Include(st => st.Movie)
                .Where(st => st.RoomID == roomId && !st.IsDeleted && st.StartTime.Date == startTime.Date);

            if (!string.IsNullOrEmpty(excludeShowTimeId))
                query = query.Where(st => st.ShowTimeID != excludeShowTimeId);

            var existingShowTimes = await query.ToListAsync();
            var conflicts = new List<ShowTime>();

            foreach (var existing in existingShowTimes)
            {
                var existingEnd = existing.StartTime.AddMinutes(existing.Movie?.DurationMinutes ?? 0);
                var existingStartBuffer = existing.StartTime.AddMinutes(-bufferMinutes);
                var existingEndBuffer = existingEnd.AddMinutes(bufferMinutes);

                if ((startTime >= existingStartBuffer && startTime < existingEndBuffer) ||
                    (endTime > existingStartBuffer && endTime <= existingEndBuffer) ||
                    (startTime <= existingStartBuffer && endTime >= existingEndBuffer))
                    conflicts.Add(existing);
            }
            return conflicts;
        }
    }
}