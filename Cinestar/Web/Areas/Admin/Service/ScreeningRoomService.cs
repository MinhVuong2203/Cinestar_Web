using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class ScreeningRoomService : IScreeningRoomService
    {
        private readonly CineStarContext _context;

        public ScreeningRoomService(CineStarContext context)
        {
            this._context = context;
        }

        // Lấy tất cả phòng chiếu (bao gồm cả đã xóa)
        public async Task<List<Room>> GetScreeningRooms()
        {
            List<Room> rooms = await _context.Rooms
                .Include(r => r.Branch)
                .OrderBy(r => r.RoomID)
                .ToListAsync();
            return rooms;
        }

        // Lấy phòng chiếu theo ID
        public async Task<Room> GetScreeningRoom(string id)
        {
            var room = await _context.Rooms
                .Include(r => r.Branch)
                .FirstOrDefaultAsync(r => r.RoomID == id);
            return room;
        }

        // Tạo phòng chiếu mới (Trigger sẽ tự động tạo RoomID)
        public async Task CreateScreeningRoom(Room room)
        {
            room.IsDeleted = false;
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        // Cập nhật phòng chiếu
        public async Task EditScreeningRoom(Room room)
        {
            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        // Xóa mềm phòng chiếu
        public async Task DeleteScreeningRoom(string roomId)
        {
            Room room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        // Khôi phục phòng chiếu đã xóa
        public async Task RestoreScreeningRoom(string roomId)
        {
            Room room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.IsDeleted = false;
                await _context.SaveChangesAsync();
            }
        }

        // Lấy danh sách chi nhánh đang hoạt động
        public async Task<List<CinemaBranch>> GetActiveBranches()
        {
            List<CinemaBranch> branches = await _context.CinemaBranches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.BranchName)
                .ToListAsync();
            return branches;
        }
    }
}
