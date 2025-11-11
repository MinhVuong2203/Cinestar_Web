using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web.Data;
using Web.Hubs;
using Web.Models;

namespace Web.Service
{
    public class MovieService_Cus : IMovieService_Cus
    {
        private readonly CineStarContext _context;

        private readonly IHubContext<SeatHub> _hubContext;

        public MovieService_Cus(CineStarContext context, IHubContext<SeatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<List<Movie>> GetNowShowingMoviesAsync(int pageSize = 12)
        {
            var now = DateTime.Now;
            Debug.WriteLine($"=== GetNowShowingMoviesAsync ===");
            Debug.WriteLine($"Current DateTime: {now}");
            Debug.WriteLine($"Query: Phim đang chiếu - StartTime <= {now} AND EndTime >= {now}");

            var movies = await _context.Movies
                .Where(m => !m.IsDeleted
                    && m.StartTime.HasValue
                    && m.EndTime.HasValue
                    && m.StartTime.Value <= now  // Đã bắt đầu chiếu
                    && m.EndTime.Value >= now)   // Chưa kết thúc
                .OrderBy(m => m.StartTime)
                .Take(pageSize)
                .ToListAsync();

            Debug.WriteLine($"Found {movies.Count} movies");
            foreach (var movie in movies)
            {
                Debug.WriteLine($"  - {movie.MovieID}: {movie.Title}");
                Debug.WriteLine($"    StartTime: {movie.StartTime}, EndTime: {movie.EndTime}");
            }

            return movies;
        }

        public async Task<List<Movie>> GetComingSoonMoviesAsync(int pageSize = 12)
        {
            var now = DateTime.Now;
            Debug.WriteLine($"=== GetComingSoonMoviesAsync ===");
            Debug.WriteLine($"Current DateTime: {now}");
            Debug.WriteLine($"Query: Phim sắp chiếu - StartTime > {now}");

            var movies = await _context.Movies
                .Where(m => !m.IsDeleted
                    && m.StartTime.HasValue
                    && m.StartTime.Value > now)  // Chưa bắt đầu chiếu
                .OrderBy(m => m.StartTime)
                .Take(pageSize)
                .ToListAsync();

            Debug.WriteLine($"Found {movies.Count} movies");
            foreach (var movie in movies)
            {
                Debug.WriteLine($"  - {movie.MovieID}: {movie.Title}");
                Debug.WriteLine($"    StartTime: {movie.StartTime}, EndTime: {movie.EndTime}");
            }

            return movies;
        }

        public async Task<Movie?> GetMovieByIdAsync(string movieId)
        {
            var movie = await _context.Movies
                .Where(m => m.MovieID == movieId && !m.IsDeleted)
                .FirstOrDefaultAsync();

            return movie;
        }

        //lấy danh sách ngày chiếu phim dd/mm/yyyy
        public List<string> GetMovieDates(string movieId)
        {
            try
            {
                var dates = _context.ShowTimes
                    .Where(st => !st.IsDeleted
                        && st.MovieID == movieId
                        && st.StartTime >= DateTime.Today)
                    .Select(st => st.StartTime.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .Select(d => d.ToString("dd/MM/yyyy"))
                    .ToList();

                return dates;
            }
            catch
            {
                return new List<string>();
            }
        }

        //lấy danh sách giờ chiếu phim theo movieId và date
        public List<string> GetMovieShowTimes(string movieId, string date)
        {
            try
            {
                DateTime parsedDate = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                var showTimes = _context.ShowTimes
                    .Where(st => !st.IsDeleted
                        && st.MovieID == movieId
                        && st.StartTime.Date == parsedDate.Date
                        && st.StartTime >= DateTime.Now)
                    .Select(st => st.StartTime.ToString("HH:mm"))
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
                return showTimes;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<Object> GetSeatingLayoutAsync(string showTimeId, Guid currentCustomerId)
        {
            var tickets = await _context.Tickets
                .Include(t => t.Seat)
                .Where(t => t.ShowTimeID == showTimeId && !t.IsDeleted)
                .Select(t => new
                {
                    seatID = t.SeatID,
                    seatName = t.Seat.SeatName,
                    seatType = t.Seat.SeatType,
                    status = t.Status,
                    isMyChoice = t.LockedBy == currentCustomerId
                }).OrderBy(t => t.seatName).ToListAsync();
            return tickets;
        }


        public async Task<bool> TrySelectSeatAsync(string showTimeId, string seatId, Guid customerId)
        {
            var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.ShowTimeID == showTimeId && t.SeatID == seatId && !t.IsDeleted);
            if (ticket == null) return false;
            // Kiểm tra ghế có trống HOẶC do chính user này đang giữ
            if (ticket.Status == "Trống" || ticket.LockedBy == customerId)
            {
                // Lock ghế cho user này
                ticket.Status = "Đang được chọn";
                ticket.LockedBy = customerId;
                ticket.LockedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group(showTimeId).SendAsync("SeatSelected", new
                {
                    seatId = seatId,
                    customerId = customerId.ToString(),
                    status = "Đang được chọn"
                });
                return true;
            }
            // OPTIONAL: Double-check nếu ghế hết hạn (phòng trường hợp Background Service bị delay)
            if (ticket.Status == "Đang được chọn" &&
                ticket.LockedAt.HasValue &&
                (DateTime.Now - ticket.LockedAt.Value).TotalMinutes >= 5)
            {
                // Giải phóng ngay lập tức
                ticket.Status = "Đang được chọn";
                ticket.LockedBy = customerId;
                ticket.LockedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            return false; // Ghế đang bị người khác giữ
        }
        public async Task<bool> DeselectSeatAsync(string showTimeId, string seatId, Guid customerId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.ShowTimeID == showTimeId &&
                    t.SeatID == seatId &&
                    t.LockedBy == customerId); // ← QUAN TRỌNG: Chỉ cho phép bỏ chọn ghế của chính mình
            if (ticket != null)
            {
                ticket.Status = "Trống";
                ticket.LockedBy = null;
                ticket.LockedAt = null;

                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group(showTimeId).SendAsync("SeatDeselected", new
                {
                    seatId = seatId,
                    status = "Trống"
                });
                return true;
            }
            return false;
        }

        public async Task<Ticket?> GetTicketBySeatIdAsync(string showTimeId, string seatId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.ShowTimeID == showTimeId && t.SeatID == seatId && !t.IsDeleted);
            return ticket;
        }

    }
}