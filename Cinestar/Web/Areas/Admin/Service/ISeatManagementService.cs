using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface ISeatManagementService
    {
        Task<List<Room>> GetAllRoomsWithSeats();
        Task<Room> GetRoomWithSeats(string roomId);
        Task<List<Seat>> GetSeatsByRoomId(string roomId);
        Task<List<CinemaBranch>> GetActiveBranches();
        Task UpdateSeat(Seat seat);
        Task DeleteSeat(string seatId);
        Task<Seat> CreateSeat(Seat seat); 
    }
}
