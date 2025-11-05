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

            // ✅ Đảm bảo SeatCount = 250 (nếu chưa có)
            if (!room.SeatCount.HasValue || room.SeatCount.Value <= 0)
            {
                room.SeatCount = 250;
            }

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            // ✅ TỰ ĐỘNG TẠO 250 GHẾ SAU KHI TẠO PHÒNG
            // Lấy RoomID vừa tạo (do trigger tạo)
            var createdRoomId = await _context.Rooms
                .AsNoTracking()
                .Where(r => r.RoomName == room.RoomName && !r.IsDeleted)
                .OrderByDescending(r => r.RoomID)
                .Select(r => r.RoomID)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(createdRoomId))
            {
                await CreateSeatsUsingRawSQL(createdRoomId);
            }
        }

        // ✅ TẠO GHẾ BẰNG RAW SQL (Tránh tracking conflict)
        private async Task CreateSeatsUsingRawSQL(string roomId)
        {
            var insertStatements = new List<string>();

            // Hàng A-F: 15 ghế mỗi hàng (6 hàng * 15 = 90 ghế)
            for (char row = 'A'; row <= 'F'; row++)
            {
                for (int col = 1; col <= 15; col++)
                {
                    var seatName = $"{row}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'Ghế thường', '{roomId}', 0)");
                }
            }

            // Hàng G-N: 17 ghế mỗi hàng (8 hàng * 17 = 136 ghế)
            for (char row = 'G'; row <= 'N'; row++)
            {
                for (int col = 1; col <= 17; col++)
                {
                    var seatName = $"{row}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'Ghế thường', '{roomId}', 0)");
                }
            }

            // Hàng O: 24 ghế (24 ghế để đủ 250)
            // Tổng: 90 + 136 + 24 = 250 ghế
            for (int col = 1; col <= 24; col++)
            {
                var seatName = $"O{col:D2}";
                insertStatements.Add($"(N'{seatName}', N'Ghế thường', '{roomId}', 0)");
            }

            // ✅ Chia nhỏ insert (SQL Server giới hạn 1000 rows/batch)
            var batchSize = 100;
            for (int i = 0; i < insertStatements.Count; i += batchSize)
            {
                var batch = insertStatements.Skip(i).Take(batchSize);
                var values = string.Join(",\n", batch);

                var sql = $@"
                    INSERT INTO Seat (SeatName, SeatType, RoomID, IsDeleted)
                    VALUES {values};
                ";

                await _context.Database.ExecuteSqlRawAsync(sql);
            }
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

        public async Task DeleteScreeningRoom(string roomId)
        {
            Room room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }


        // Khôi phục phòng chiếu đã xóa
        //public async Task RestoreScreeningRoom(string roomId)
        //{
        //    Room room = await _context.Rooms.FindAsync(roomId);
        //    if (room != null)
        //    {
        //        room.IsDeleted = false;
        //        await _context.SaveChangesAsync();
        //    }
        //}

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
