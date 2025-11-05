using Microsoft.EntityFrameworkCore;
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

        // ===== CRUD OPERATIONS =====

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
                // Kiểm tra conflict trước khi tạo
                var movie = await GetMovieByIdAsync(showTime.MovieID);
                if (movie == null) return false;

                var endTime = showTime.StartTime.AddMinutes(movie.DurationMinutes ?? 0);
                var hasConflict = await CheckTimeConflictAsync(showTime.RoomID, showTime.StartTime, endTime);

                if (hasConflict) return false;

                _db.ShowTimes.Add(showTime);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ShowTime showTime)
        {
            try
            {
                // Kiểm tra conflict trước khi update (exclude ID hiện tại)
                var movie = await GetMovieByIdAsync(showTime.MovieID);
                if (movie == null) return false;

                var endTime = showTime.StartTime.AddMinutes(movie.DurationMinutes ?? 0);
                var hasConflict = await CheckTimeConflictAsync(
                    showTime.RoomID,
                    showTime.StartTime,
                    endTime,
                    showTime.ShowTimeID
                );

                if (hasConflict) return false;

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
            _db.ShowTimes.Update(st);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(string id)
        {
            var st = await _db.ShowTimes.FindAsync(id);
            if (st == null) return false;
            st.IsDeleted = false;
            _db.ShowTimes.Update(st);
            await _db.SaveChangesAsync();
            return true;
        }

        // ===== DROPDOWN DATA =====

        public async Task<List<Movie>> GetAllMoviesAsync()
            => await _db.Movies
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Title)
                .ToListAsync();

        public async Task<List<CinemaBranch>> GetAllBranchesAsync()
            => await _db.CinemaBranches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.BranchName)
                .ToListAsync();

        public async Task<List<Room>> GetRoomsByBranchAsync(string branchId)
            => await _db.Rooms
                .Where(r => r.BranchID == branchId && !r.IsDeleted)
                .OrderBy(r => r.RoomName)
                .ToListAsync();

        public async Task<Movie?> GetMovieByIdAsync(string movieId)
            => await _db.Movies.FindAsync(movieId);

        // ===== TIMELINE & CONFLICT DETECTION =====

        /// <summary>
        /// Lấy tất cả suất chiếu của phòng trong 1 ngày (để hiển thị timeline)
        /// </summary>
        public async Task<List<ShowTime>> GetShowTimesByRoomAndDateAsync(string roomId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _db.ShowTimes
                .Include(x => x.Movie)
                .Where(st => st.RoomID == roomId
                    && !st.IsDeleted
                    && st.StartTime >= startOfDay
                    && st.StartTime < endOfDay)
                .OrderBy(st => st.StartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra xem có conflict về thời gian không
        /// Phải cách nhau ít nhất 15 phút (buffer time)
        /// </summary>
        public async Task<bool> CheckTimeConflictAsync(
            string roomId,
            DateTime startTime,
            DateTime endTime,
            string? excludeShowTimeId = null)
        {
            // Buffer time: 15 phút trước và sau mỗi suất chiếu
            const int bufferMinutes = 15;

            // Lấy tất cả suất chiếu trong phòng vào ngày đó (không bao gồm đã xóa)
            var query = _db.ShowTimes
                .Include(st => st.Movie)
                .Where(st => st.RoomID == roomId
                    && !st.IsDeleted
                    && st.StartTime.Date == startTime.Date); // Chỉ lấy cùng ngày

            // Nếu đang edit, loại trừ showtime hiện tại
            if (!string.IsNullOrEmpty(excludeShowTimeId))
            {
                query = query.Where(st => st.ShowTimeID != excludeShowTimeId);
            }

            var existingShowTimes = await query.ToListAsync();

            foreach (var existingShowTime in existingShowTimes)
            {
                // Tính thời gian kết thúc của suất chiếu đã có
                var existingEndTime = existingShowTime.StartTime
                    .AddMinutes(existingShowTime.Movie?.DurationMinutes ?? 0);

                // Tính buffer zones
                var existingStartWithBuffer = existingShowTime.StartTime.AddMinutes(-bufferMinutes);
                var existingEndWithBuffer = existingEndTime.AddMinutes(bufferMinutes);

                // Kiểm tra overlap:
                // Case 1: Suất mới bắt đầu trong khoảng buffer của suất cũ
                bool newStartInBuffer = startTime >= existingStartWithBuffer
                    && startTime < existingEndWithBuffer;

                // Case 2: Suất mới kết thúc trong khoảng buffer của suất cũ
                bool newEndInBuffer = endTime > existingStartWithBuffer
                    && endTime <= existingEndWithBuffer;

                // Case 3: Suất mới bao trùm hoàn toàn suất cũ (kể cả buffer)
                bool newCoversExisting = startTime <= existingStartWithBuffer
                    && endTime >= existingEndWithBuffer;

                if (newStartInBuffer || newEndInBuffer || newCoversExisting)
                {
                    return true; // Có conflict
                }
            }

            return false; // Không có conflict
        }

        /// <summary>
        /// Lấy thông tin chi tiết về conflict (dùng cho thông báo lỗi)
        /// </summary>
        public async Task<List<ShowTime>> GetConflictingShowTimesAsync(
            string roomId,
            DateTime startTime,
            DateTime endTime,
            string? excludeShowTimeId = null)
        {
            const int bufferMinutes = 15;

            var query = _db.ShowTimes
                .Include(st => st.Movie)
                .Where(st => st.RoomID == roomId
                    && !st.IsDeleted
                    && st.StartTime.Date == startTime.Date);

            if (!string.IsNullOrEmpty(excludeShowTimeId))
            {
                query = query.Where(st => st.ShowTimeID != excludeShowTimeId);
            }

            var existingShowTimes = await query.ToListAsync();
            var conflicts = new List<ShowTime>();

            foreach (var existingShowTime in existingShowTimes)
            {
                var existingEndTime = existingShowTime.StartTime
                    .AddMinutes(existingShowTime.Movie?.DurationMinutes ?? 0);

                var existingStartWithBuffer = existingShowTime.StartTime.AddMinutes(-bufferMinutes);
                var existingEndWithBuffer = existingEndTime.AddMinutes(bufferMinutes);

                bool hasConflict =
                    (startTime >= existingStartWithBuffer && startTime < existingEndWithBuffer) ||
                    (endTime > existingStartWithBuffer && endTime <= existingEndWithBuffer) ||
                    (startTime <= existingStartWithBuffer && endTime >= existingEndWithBuffer);

                if (hasConflict)
                {
                    conflicts.Add(existingShowTime);
                }
            }

            return conflicts;
        }
    }
}