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
                await CreateSeatsUsingRawSQL(createdRoomId, room.SeatCount ?? 250); // Truyền seatCount thực sự ở đây!
            }

        }

        // ✅ TẠO GHẾ BẰNG RAW SQL (250 ghế đúng logic)
        private async Task CreateSeatsUsingRawSQL(string roomId, int seatCount)
        {
            var insertStatements = new List<string>();
            int totalSeats = 0;

            // GHẾ ĐÔI - A,B: 15 ghế/hàng
            for (char row = 'A'; row <= 'B' && totalSeats < seatCount; row++)
            {
                for (int col = 1; col <= 15 && totalSeats < seatCount; col++)
                {
                    string seatName = $"{row}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'Ghế đôi', '{roomId}', 0)");
                    totalSeats++;
                }
            }

            // C-E: thường, 15 ghế/hàng
            for (char row = 'C'; row <= 'E' && totalSeats < seatCount; row++)
            {
                for (int col = 1; col <= 15 && totalSeats < seatCount; col++)
                {
                    string seatName = $"{row}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'Ghế thường', '{roomId}', 0)");
                    totalSeats++;
                }
            }

            // F: 15 ghế, VIP từ F03-F13
            if (totalSeats < seatCount)
            {
                char row = 'F';
                for (int col = 1; col <= 15 && totalSeats < seatCount; col++)
                {
                    string seatType = (col >= 3 && col <= 13) ? "Ghế VIP" : "Ghế thường";
                    string seatName = $"{row}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'{seatType}', '{roomId}', 0)");
                    totalSeats++;
                }
            }

            // G-L: 17 ghế/hàng, VIP từ col 4-14
            for (char row = 'G'; row <= 'L' && totalSeats < seatCount; row++)
            {
                for (int col = 1; col <= 17 && totalSeats < seatCount; col++)
                {
                    string seatType = (col >= 4 && col <= 14) ? "Ghế VIP" : "Ghế thường";
                    string seatName = $"{row}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'{seatType}', '{roomId}', 0)");
                    totalSeats++;
                }
            }

            // Nếu vẫn chưa đủ, sinh tiếp các hàng thường (M, N... 17 ghế/hàng)
            char addRow = 'M';
            while (totalSeats < seatCount && addRow <= 'Z')
            {
                for (int col = 1; col <= 17 && totalSeats < seatCount; col++)
                {
                    string seatName = $"{addRow}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'Ghế thường', '{roomId}', 0)");
                    totalSeats++;
                }
                addRow++;
            }

            // Batched Insert như cũ
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
