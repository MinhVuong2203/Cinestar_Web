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
    public class ScreeningRoomController : Controller
    {
        private readonly IScreeningRoomService _screeningRoomService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CineStarContext _context;


        public ScreeningRoomController(
            IScreeningRoomService screeningRoomService,
            IWebHostEnvironment webHostEnvironment,
            CineStarContext context)
        {
            _screeningRoomService = screeningRoomService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [Authorize(Roles = "Admin, EmployeeTechnician")]
        public async Task<IActionResult> Index()
        {
            var rooms = await _screeningRoomService.GetScreeningRooms();
            ViewBag.TotalRooms = rooms.Count;
            ViewBag.TotalSeats = rooms.Sum(r => r.SeatCount ?? 0);
            ViewBag.AllBranches = await _screeningRoomService.GetActiveBranches();

            return View(rooms);
        }


        [Authorize(Roles = "Admin, EmployeeTechnician")]
        public async Task<IActionResult> Create()
        {
            await LoadBranchesForView();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, EmployeeTechnician")]
        public async Task<IActionResult> Create(Room room, IFormFile? ImageFile)
        {
            try
            {
                ModelState.Remove("RoomID");
                bool isDuplicate = await _context.Rooms
                    .AnyAsync(r =>
                        r.RoomName.ToLower() == room.RoomName.ToLower()
                        && r.BranchID == room.BranchID);
                if (isDuplicate)
                {
                    ModelState.AddModelError("RoomName", "Tên phòng này đã tồn tại trong chi nhánh.");
                    await LoadBranchesForView();
                    return View(room);
                }

                if (ModelState.IsValid)
                {
                    if (!room.SeatCount.HasValue || room.SeatCount.Value <= 0)
                    {
                        room.SeatCount = 250;
                    }
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        room.ImageUrl = await UploadImageAsync(ImageFile);
                    }
                    await _screeningRoomService.CreateScreeningRoom(room);
                    TempData["Success"] = "Thêm phòng chiếu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                await LoadBranchesForView();
                return View(room);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                await LoadBranchesForView();
                return View(room);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _screeningRoomService.GetScreeningRoom(id);
            if (room == null)
            {
                return NotFound();
            }

            await LoadBranchesForView();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Room room, IFormFile? ImageFile)
        {
            if (id != room.RoomID)
            {
                return NotFound();
            }

            try
            {
                bool isDuplicate = await _context.Rooms
                    .AnyAsync(r =>
                        r.RoomName.ToLower() == room.RoomName.ToLower()
                        && r.BranchID == room.BranchID
                        && r.RoomID != room.RoomID);
                if (isDuplicate)
                {
                    ModelState.AddModelError("RoomName", "Tên phòng này đã tồn tại trong chi nhánh.");
                    await LoadBranchesForView();
                    return View(room);
                }

                if (ModelState.IsValid)
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(room.ImageUrl))
                        {
                            DeleteImage(room.ImageUrl);
                        }
                        room.ImageUrl = await UploadImageAsync(ImageFile);
                    }
                    await _screeningRoomService.EditScreeningRoom(room);
                    TempData["Success"] = "Cập nhật phòng chiếu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                await LoadBranchesForView();
                return View(room);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                await LoadBranchesForView();
                return View(room);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _screeningRoomService.DeleteScreeningRoom(id);
                TempData["Success"] = "Xóa phòng chiếu vĩnh viễn thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBranchesForView()
        {
            var branches = await _screeningRoomService.GetActiveBranches();
            ViewBag.Branches = branches;
        }

        private async Task<string> UploadImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/rooms/" + uniqueFileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        public async Task<IActionResult> Chart()
        {
            ViewBag.AllBranches = await _screeningRoomService.GetActiveBranches();
            ViewBag.AllRooms = await _screeningRoomService.GetScreeningRooms();
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetRoomChartData(string branchId, DateTime fromDate, DateTime toDate)
        {
            var data = await _screeningRoomService.GetTopRoomsByBranch(branchId, fromDate, toDate);
            var labels = data.Select(x => x.RoomName).ToArray();
            var values = data.Select(x => x.TicketCount).ToArray();

            return Json(new { labels, values });
        }

        [HttpPost]
        public async Task<JsonResult> GetSeatChartData(string roomId, DateTime fromDate, DateTime toDate)
        {
            var data = await _screeningRoomService.GetTopSeatsByRoom(roomId, fromDate, toDate);
            var labels = data.Select(x => x.SeatName).ToArray();
            var values = data.Select(x => x.TicketCount).ToArray();

            return Json(new { labels, values });
        }
    }
}
