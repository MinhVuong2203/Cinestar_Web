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

        public async Task<List<Room>> GetAllRoomsWithSeats()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Branch)
                .Include(r => r.Seats) 
                .OrderBy(r => r.RoomName)
                .ToListAsync();

            return rooms;
        }

        public async Task<Room> GetRoomWithSeats(string roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Branch)
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.RoomID == roomId);

            return room;
        }

        public async Task<List<Seat>> GetSeatsByRoomId(string roomId)
        {
            return await _context.Seats
                .Where(s => s.RoomID == roomId)
                .OrderBy(s => s.SeatName)
                .ToListAsync();
        }

        public async Task<List<CinemaBranch>> GetActiveBranches()
        {
            return await _context.CinemaBranches
                .OrderBy(b => b.BranchName)
                .ToListAsync();
        }
        public async Task<Seat> CreateSeat(Seat seat)
        {
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
        public async Task UpdateSeat(Seat seat)
        {
            var existingSeat = await _context.Seats.FindAsync(seat.SeatID);
            if (existingSeat != null)
            {
                existingSeat.SeatType = seat.SeatType;
                existingSeat.SeatName = seat.SeatName;
                await _context.SaveChangesAsync();
            }
        }
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
