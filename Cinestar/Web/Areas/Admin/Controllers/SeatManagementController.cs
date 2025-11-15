using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Admin.Service;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, EmployeeTechnician")]
    public class SeatManagementController : Controller
    {
        private readonly ISeatManagementService _seatManagementService;
        private readonly CineStarContext _context; 

        public SeatManagementController(
            ISeatManagementService seatManagementService,
            CineStarContext context) 
        {
            _seatManagementService = seatManagementService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _seatManagementService.GetAllRoomsWithSeats();

            ViewBag.TotalRooms = rooms.Count;
            ViewBag.TotalSeats = rooms.Sum(r => r.Seats?.Count ?? 0);
            ViewBag.AllBranches = await _seatManagementService.GetActiveBranches();

            return View(rooms);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var room = await _seatManagementService.GetRoomWithSeats(id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSeat(string roomId, string seatName, string seatType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(seatName))
                {
                    return Json(new { success = false, message = "Tên ghế không được để trống!" });
                }

                var newSeat = new Seat
                {
                    SeatName = seatName.Trim().ToUpper(),
                    SeatType = seatType,
                    RoomID = roomId
                };

                var createdSeat = await _seatManagementService.CreateSeat(newSeat);

                return Json(new
                {
                    success = true,
                    message = "Thêm ghế thành công!",
                    seatId = createdSeat.SeatID,
                    seatName = createdSeat.SeatName,
                    reload = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSeat(string seatId, string seatName, string seatType)
        {
            try
            {
                var seat = await _context.Seats.FirstOrDefaultAsync(s => s.SeatID == seatId);

                if (seat != null)
                {
                    seat.SeatName = seatName; 
                    seat.SeatType = seatType;
                    _context.Update(seat);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }

                return Json(new { success = false, message = "Không tìm thấy ghế!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSeat(string seatId)
        {
            try
            {
                var hasTickets = await _context.Tickets.AnyAsync(t => t.SeatID == seatId);

                if (hasTickets)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa ghế đã có vé đặt!"
                    });
                }

                await _seatManagementService.DeleteSeat(seatId);

                return Json(new
                {
                    success = true,
                    message = "Xóa ghế thành công!",
                    reload = true  
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
