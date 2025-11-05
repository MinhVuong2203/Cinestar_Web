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
    }
}
