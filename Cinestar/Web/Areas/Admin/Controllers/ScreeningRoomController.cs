using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ScreeningRoomController : Controller
    {
        private readonly IScreeningRoomService _screeningRoomService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ScreeningRoomController(
            IScreeningRoomService screeningRoomService,
            IWebHostEnvironment webHostEnvironment)
        {
            _screeningRoomService = screeningRoomService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/ScreeningRoom/Index
        public async Task<IActionResult> Index()
        {
            var rooms = await _screeningRoomService.GetScreeningRooms();

            // Thống kê
            ViewBag.TotalRooms = rooms.Count;
            ViewBag.ActiveRooms = rooms.Count(r => !r.IsDeleted);
            ViewBag.DeletedRooms = rooms.Count(r => r.IsDeleted);
            ViewBag.TotalSeats = rooms.Where(r => !r.IsDeleted).Sum(r => r.SeatCount ?? 0);

            return View(rooms);
        }

        // GET: Admin/ScreeningRoom/Create
        public async Task<IActionResult> Create()
        {
            await LoadBranchesForView();
            return View();
        }

        // POST: Admin/ScreeningRoom/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, IFormFile? ImageFile)
        {
            try
            {
                // Bỏ validation cho RoomID vì trigger sẽ tự tạo
                ModelState.Remove("RoomID");

                if (ModelState.IsValid)
                {
                    // Upload ảnh nếu có
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

        // GET: Admin/ScreeningRoom/Edit/ROM-12345
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

        // POST: Admin/ScreeningRoom/Edit/ROM-12345
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
                if (ModelState.IsValid)
                {
                    // Upload ảnh mới nếu có
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
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

        // POST: Admin/ScreeningRoom/Delete/ROM-12345
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _screeningRoomService.DeleteScreeningRoom(id);
                TempData["Success"] = "Xóa phòng chiếu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ScreeningRoom/Restore/ROM-12345
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            try
            {
                await _screeningRoomService.RestoreScreeningRoom(id);
                TempData["Success"] = "Khôi phục phòng chiếu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== HELPER METHODS =====

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
    }
}
