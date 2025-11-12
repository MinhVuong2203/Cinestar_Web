using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface IScreeningRoomService
    {
        Task<List<Room>> GetScreeningRooms();
        Task<Room> GetScreeningRoom(string id);
        Task CreateScreeningRoom(Room room);
        Task EditScreeningRoom(Room room);
        Task DeleteScreeningRoom(string roomId);
        Task<List<CinemaBranch>> GetActiveBranches();

        Task<List<(string RoomName, int TicketCount)>> GetTopRoomsByBranch(string branchId, DateTime fromDate, DateTime toDate);
        Task<List<(string SeatName, int TicketCount)>> GetTopSeatsByRoom(string roomId, DateTime fromDate, DateTime toDate);
    }
}
