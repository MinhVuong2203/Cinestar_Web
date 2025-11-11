using Web.Models;

namespace Web.Service
{
    public interface IShowTimeService
    {
        /// <summary>
        /// Lấy danh sách lịch chiếu theo rạp, phim và ngày
        /// </summary>
        List<ShowTimeGroup> GetShowTimesByBranchMovieDate(string branchId, string movieId, DateTime date);

        /// <summary>
        /// Lấy thông tin giá vé theo suất chiếu
        /// </summary>
        List<TicketPriceInfo> GetTicketPricesByShowTime(string showTimeId);
    }

    /// <summary>
    /// Model nhóm lịch chiếu theo loại phòng
    /// </summary>
    public class ShowTimeGroup
    {
        public string RoomType { get; set; } = string.Empty;
        public List<ShowTimeInfo> ShowTimes { get; set; } = new();
    }

    /// <summary>
    /// Thông tin suất chiếu
    /// </summary>
    public class ShowTimeInfo
    {
        public string ShowTimeID { get; set; } = string.Empty;
        public string TimeDisplay { get; set; } = string.Empty; // VD: "14:30"
        public decimal BasePrice { get; set; }

        public string nameRoom { get; set; }
    }

    /// <summary>
    /// Thông tin giá vé
    /// </summary>
    public class TicketPriceInfo
    {
public string TicketType { get; set; } = string.Empty; // VD: "Người lớn", "Trẻ em"
   public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableCount { get; set; }
}
}
