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

            //TỰ ĐỘNG TẠO 250 GHẾ SAU KHI TẠO PHÒNG
            var createdRoomId = await _context.Rooms
                .AsNoTracking()
                .Where(r => r.RoomName == room.RoomName && !r.IsDeleted)
                .OrderByDescending(r => r.RoomID)
                .Select(r => r.RoomID)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(createdRoomId))
            {
                await CreateSeatsUsingRawSQL(createdRoomId, room.SeatCount ?? 250);
            }

        }

        // tạo ghế cho phòng
        private async Task CreateSeatsUsingRawSQL(string roomId, int seatCount)
        {
            var insertStatements = new List<string>();
            int totalSeats = 0;

            // Define danh sách hàng ghế (A→P, Q, R, ... tùy thực tế, hoặc tự sinh đến đủ seatCount)
            List<char> rowList = new List<char>();
            for (char row = 'A'; totalSeats < seatCount && row <= 'Z'; row++)
            {
                rowList.Add(row);
                int seatPerRow = (row >= 'G') ? 17 : 15;
                totalSeats += seatPerRow;
            }

            // Xác định 2 hàng cuối cùng làm couple
            int rowCount = rowList.Count;
            char coupleRow1 = rowList[^1];
            char coupleRow2 = rowList[^2];

            totalSeats = 0;

            for (int i = 0; i < rowCount && totalSeats < seatCount; i++)
            {
                char row = rowList[i];
                int seatPerRow = (row >= 'G') ? 17 : 15;

                for (int col = 1; col <= seatPerRow && totalSeats < seatCount; col++)
                {
                    string seatType = "Ghế thường";

                    // 2 hàng cuối (xa màn hình nhất) là couple
                    if (row == coupleRow1 || row == coupleRow2)
                        seatType = "Ghế đôi";
                    // VIP: L->G col 4-14, F: col 3-13
                    else if (row >= 'G' && row <= 'L' && col >= 4 && col <= 14)
                        seatType = "Ghế VIP";
                    else if (row == 'F' && col >= 3 && col <= 13)
                        seatType = "Ghế VIP";

                    var seatName = $"{row}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'{seatType}', '{roomId}', 0)");
                    totalSeats++;
                }
            }

            char nextRow = (char)(rowList.Last() + 1);
            while (totalSeats < seatCount && nextRow <= 'Z')
            {
                for (int col = 1; col <= 17 && totalSeats < seatCount; col++)
                {
                    var seatName = $"{nextRow}{col:D2}";
                    insertStatements.Add($"(N'{seatName}', N'Ghế thường', '{roomId}', 0)");
                    totalSeats++;
                }
                nextRow++;
            }
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
