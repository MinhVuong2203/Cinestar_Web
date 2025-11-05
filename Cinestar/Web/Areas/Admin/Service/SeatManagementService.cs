using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class SeatManagementService : ISeatManagementService
    {
        private readonly CineStarContext _context;

        public SeatManagementService(CineStarContext context)
        {
            this._context = context;
        }

        // Lấy tất cả phòng với số ghế
        public async Task<List<Room>> GetAllRoomsWithSeats()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Branch)
                .Include(r => r.Seats) // Load tất cả ghế
                .OrderBy(r => r.RoomName)
                .ToListAsync();

            return rooms;
        }

        // Lấy phòng với danh sách ghế
        public async Task<Room> GetRoomWithSeats(string roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Branch)
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.RoomID == roomId);

            return room;
        }

        // Lấy danh sách ghế theo phòng
        public async Task<List<Seat>> GetSeatsByRoomId(string roomId)
        {
            return await _context.Seats
                .Where(s => s.RoomID == roomId)
                .OrderBy(s => s.SeatName)
                .ToListAsync();
        }

        // Lấy danh sách chi nhánh
        public async Task<List<CinemaBranch>> GetActiveBranches()
        {
            return await _context.CinemaBranches
                .OrderBy(b => b.BranchName)
                .ToListAsync();
        }
        public async Task<Seat> CreateSeat(Seat seat)
        {
            // Kiểm tra tên ghế đã tồn tại trong phòng chưa
            var exists = await _context.Seats
                .AnyAsync(s => s.SeatName == seat.SeatName && s.RoomID == seat.RoomID);

            if (exists)
            {
                throw new Exception($"Ghế {seat.SeatName} đã tồn tại trong phòng này!");
            }

            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            return seat;
        }
        // Cập nhật ghế
        public async Task UpdateSeat(Seat seat)
        {
            var existingSeat = await _context.Seats.FindAsync(seat.SeatID);
            if (existingSeat != null)
            {
                existingSeat.SeatType = seat.SeatType;
                existingSeat.SeatName = seat.SeatName; // ✅ Thêm cập nhật tên
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Xóa ghế
        public async Task DeleteSeat(string seatId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat != null)
            {
                _context.Seats.Remove(seat);
                await _context.SaveChangesAsync();
            }
        }
    }
}
