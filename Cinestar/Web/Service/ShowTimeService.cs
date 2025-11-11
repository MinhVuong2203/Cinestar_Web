using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;
using System.Text;

namespace Web.Service
{
    public class ShowTimeService : IShowTimeService
    {
        private readonly CineStarContext _context;

        public ShowTimeService(CineStarContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách lịch chiếu theo rạp, phim và ngày (nhóm theo loại phòng)
        /// </summary>
        public List<ShowTimeGroup> GetShowTimesByBranchMovieDate(string branchId, string movieId, DateTime date)
        {
            try
            {
                var showTimes = _context.ShowTimes
                    .Include(st => st.Room)
                    .Where(st => !st.IsDeleted
                        && st.Room.BranchID == branchId
                        && st.MovieID == movieId
                        && st.StartTime.Date == date.Date
                        && st.StartTime >= DateTime.Now) // Chỉ lấy suất chiếu chưa qua
                    .OrderBy(st => st.StartTime)
                    .ToList();

                // Nhóm theo loại phòng
                var grouped = showTimes
                    .GroupBy(st => st.Room.RoomType ?? "Standard")
                    .Select(g => new ShowTimeGroup
                    {
                        RoomType = g.Key,
                        ShowTimes = g.Select(st => new ShowTimeInfo
                        {
                            ShowTimeID = st.ShowTimeID,
                            TimeDisplay = st.StartTime.ToString("HH:mm"),
                            nameRoom = st.Room.RoomName,
                            BasePrice = st.Price ?? 0
                        }).ToList()
                    })
                    .ToList();

                return grouped;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetShowTimesByBranchMovieDate: {ex.Message}");
                return new List<ShowTimeGroup>();
            }
        }

        /// <summary>
        /// Lấy thông tin giá vé theo suất chiếu
        /// </summary>
        public List<TicketPriceInfo> GetTicketPricesByShowTime(string showTimeId)
        {
            try
            {
                var showTime = _context.ShowTimes
                    .Include(st => st.Room)
                    .FirstOrDefault(st => st.ShowTimeID == showTimeId && !st.IsDeleted);

                if (showTime == null)
                    return new List<TicketPriceInfo>();

                // Lấy số ghế đã đặt
                var bookedSeats = _context.Tickets
                    .Count(t => t.ShowTimeID == showTimeId && !t.IsDeleted);

                // Tổng số ghế
                var totalSeats = showTime.Room?.SeatCount ?? 0;
                var availableSeats = totalSeats - bookedSeats;

                // Tạo danh sách giá vé với encoding UTF-8 đúng
                var prices = new List<TicketPriceInfo>
                {
                    new TicketPriceInfo
                    {
                        TicketType = "Người lớn",      // Đảm bảo text UTF-8
                        Description = "Ghế thường",    // Đảm bảo text UTF-8
                        Price = showTime.Price ?? 0,
                        AvailableCount = availableSeats
                    },
                    new TicketPriceInfo
                    {
                        TicketType = "Sinh viên",      // Thêm loại vé khác
                        Description = "Giảm giá cho sinh viên",
                        Price = (showTime.Price ?? 0) * 0.8m, // Giảm 20%
                        AvailableCount = availableSeats
                    }
                };

                return prices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTicketPricesByShowTime: {ex.Message}");
                return new List<TicketPriceInfo>();
            }
        }
    }
}
