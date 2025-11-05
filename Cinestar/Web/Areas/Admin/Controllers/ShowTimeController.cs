using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;
using Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShowTimeController : Controller
    {
        private readonly IShowTimeService _service;

        public ShowTimeController(IShowTimeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _service.GetAllAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(string id)
        {
            var st = await _service.GetByIdAsync(id);
            if (st == null) return NotFound();
            return View(st);
        }

        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View(new ShowTime());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowTime model)
        {
            // Loại bỏ validation cho các trường tự động
            ModelState.Remove("ShowTimeID");
            ModelState.Remove("Movie");
            ModelState.Remove("Room");

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(model);
            }

            // Lấy thông tin phim để tính endTime
            var movie = await _service.GetMovieByIdAsync(model.MovieID);
            if (movie == null)
            {
                ModelState.AddModelError("MovieID", "Phim không tồn tại");
                await LoadSelectLists();
                return View(model);
            }

            // Kiểm tra thời gian bắt đầu phải trong tương lai
            if (model.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Giờ bắt đầu phải trong tương lai");
                await LoadSelectLists();
                return View(model);
            }

            // Kiểm tra conflict
            var endTime = model.StartTime.AddMinutes(movie.DurationMinutes ?? 0);
            var hasConflict = await _service.CheckTimeConflictAsync(
                model.RoomID,
                model.StartTime,
                endTime
            );

            if (hasConflict)
            {
                // Lấy thông tin chi tiết về các suất chiếu bị conflict
                var conflicts = await _service.GetConflictingShowTimesAsync(
                    model.RoomID,
                    model.StartTime,
                    endTime
                );

                var conflictMessages = conflicts.Select(c =>
                    $"{c.Movie?.Title}: {c.StartTime:HH:mm} - {c.StartTime.AddMinutes(c.Movie?.DurationMinutes ?? 0):HH:mm}"
                );

                ModelState.AddModelError("StartTime",
                    $"Khung giờ này bị trùng hoặc chưa đủ khoảng cách 15 phút với các suất chiếu: {string.Join(", ", conflictMessages)}");

                await LoadSelectLists();
                return View(model);
            }

            // Tạo mới suất chiếu
            var result = await _service.CreateAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = $"Thêm suất chiếu thành công! Phim: {movie.Title}, Giờ: {model.StartTime:dd/MM/yyyy HH:mm}";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Thêm suất chiếu thất bại. Vui lòng thử lại.");
            await LoadSelectLists();
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var st = await _service.GetByIdAsync(id);
            if (st == null) return NotFound();

            await LoadSelectLists();
            return View(st);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ShowTime model)
        {
            if (id != model.ShowTimeID) return BadRequest();

            ModelState.Remove("Movie");
            ModelState.Remove("Room");

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(model);
            }

            // Lấy thông tin phim
            var movie = await _service.GetMovieByIdAsync(model.MovieID);
            if (movie == null)
            {
                ModelState.AddModelError("MovieID", "Phim không tồn tại");
                await LoadSelectLists();
                return View(model);
            }

            // Kiểm tra conflict (exclude current showtime)
            var endTime = model.StartTime.AddMinutes(movie.DurationMinutes ?? 0);
            var hasConflict = await _service.CheckTimeConflictAsync(
                model.RoomID,
                model.StartTime,
                endTime,
                model.ShowTimeID // Loại trừ suất chiếu hiện tại
            );

            if (hasConflict)
            {
                var conflicts = await _service.GetConflictingShowTimesAsync(
                    model.RoomID,
                    model.StartTime,
                    endTime,
                    model.ShowTimeID
                );

                var conflictMessages = conflicts.Select(c =>
                    $"{c.Movie?.Title}: {c.StartTime:HH:mm} - {c.StartTime.AddMinutes(c.Movie?.DurationMinutes ?? 0):HH:mm}"
                );

                ModelState.AddModelError("StartTime",
                    $"Khung giờ này bị trùng hoặc chưa đủ khoảng cách 15 phút với các suất chiếu: {string.Join(", ", conflictMessages)}");

                await LoadSelectLists();
                return View(model);
            }

            // Cập nhật
            var result = await _service.UpdateAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = "Cập nhật suất chiếu thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Cập nhật thất bại. Vui lòng thử lại.");
            await LoadSelectLists();
            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var st = await _service.GetByIdAsync(id);
            if (st == null) return NotFound();
            return View(st);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _service.SoftDeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa suất chiếu thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _service.RestoreAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Khôi phục suất chiếu thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== API METHODS FOR AJAX =====

        /// <summary>
        /// API lấy danh sách phòng theo chi nhánh
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetRoomsByBranch(string branchId)
        {
            var rooms = await _service.GetRoomsByBranchAsync(branchId);
            return Json(rooms.Select(r => new { value = r.RoomID, text = r.RoomName }));
        }

        /// <summary>
        /// API lấy thời lượng phim
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetMovieDuration(string movieId)
        {
            var movie = await _service.GetMovieByIdAsync(movieId);
            return Json(new { duration = movie?.DurationMinutes ?? 0 });
        }

        /// <summary>
        /// API lấy timeline của phòng theo ngày
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetRoomTimeline(string roomId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                {
                    return Json(new { success = false, message = "Ngày không hợp lệ" });
                }

                var showTimes = await _service.GetShowTimesByRoomAndDateAsync(roomId, selectedDate);

                var data = showTimes.Select(st => new
                {
                    showTimeId = st.ShowTimeID,
                    movieTitle = st.Movie?.Title ?? "N/A",
                    startTime = st.StartTime.ToString("HH:mm"),
                    endTime = st.StartTime.AddMinutes(st.Movie?.DurationMinutes ?? 0).ToString("HH:mm"),
                    startDateTime = st.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    endDateTime = st.StartTime.AddMinutes(st.Movie?.DurationMinutes ?? 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                    durationMinutes = st.Movie?.DurationMinutes ?? 0
                }).ToList();

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API kiểm tra conflict về thời gian
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CheckTimeConflict(string roomId, DateTime startTime, int duration, string? excludeShowTimeId = null)
        {
            try
            {
                var endTime = startTime.AddMinutes(duration);
                var hasConflict = await _service.CheckTimeConflictAsync(roomId, startTime, endTime, excludeShowTimeId);

                return Json(new { hasConflict });
            }
            catch (Exception ex)
            {
                return Json(new { hasConflict = true, error = ex.Message });
            }
        }

        // Helper method để load SelectLists
        private async Task LoadSelectLists()
        {
            var movies = await _service.GetAllMoviesAsync();
            var branches = await _service.GetAllBranchesAsync();

            ViewBag.Movies = new SelectList(movies, "MovieID", "Title");
            ViewBag.Branches = new SelectList(branches, "BranchID", "BranchName");
        }
    }
}