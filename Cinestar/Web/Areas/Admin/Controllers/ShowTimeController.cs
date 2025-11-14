using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Areas.Admin.Service;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, EmployeeMovies")]
    public class ShowTimeController : Controller
    {
        private readonly IShowTimeService _service;

        public ShowTimeController(IShowTimeService service)
        {
            _service = service;
        }

        // Index with pagination
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            string? branchId = null,
            string? movieId = null,
            string? roomId = null)
        {
            var pagedResult = await _service.GetShowTimesPagedAsync(pageNumber, pageSize, branchId, movieId, roomId);

            ViewBag.Movies = new SelectList(await _service.GetAllMoviesAsync(), "MovieID", "Title", movieId);
            ViewBag.Branches = new SelectList(await _service.GetAllBranchesAsync(), "BranchID", "BranchName", branchId);

            ViewBag.CurrentBranchId = branchId;
            ViewBag.CurrentMovieId = movieId;
            ViewBag.CurrentRoomId = roomId;
            ViewBag.CurrentPageSize = pageSize;

            return View(pagedResult);
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
            return View(new ShowTime { IsDeleted = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowTime model)
        {
            model.IsDeleted = false;

            ModelState.Remove("ShowTimeID");
            ModelState.Remove("Movie");
            ModelState.Remove("Room");
            ModelState.Remove("Tickets");

            // ✅ VALIDATION 1: Giờ bắt đầu phải >= giờ hiện tại
            if (model.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Giờ bắt đầu phải lớn hơn hoặc bằng giờ hiện tại!");
            }

            // ✅ VALIDATION 2: Giờ bắt đầu phải >= Ngày khởi chiếu của phim (Movie.StartTime)
            var movie = await _service.GetMovieByIdAsync(model.MovieID);
            if (movie != null && movie.StartTime.HasValue)
            {
                if (model.StartTime.Date < movie.StartTime.Value.Date)
                {
                    ModelState.AddModelError("StartTime",
                        $"Giờ bắt đầu phải từ ngày khởi chiếu của phim ({movie.StartTime.Value:dd/MM/yyyy}) trở đi!");
                }
            }

            // ✅ VALIDATION 3: Kiểm tra thời gian nằm trong giờ hoạt động của chi nhánh
            var room = await _service.GetRoomByIdAsync(model.RoomID);
            if (room?.Branch != null && movie != null && movie.DurationMinutes.HasValue)
            {
                var branch = room.Branch;

                if (branch.OpenHour != default && branch.CloseHour != default)
                {
                    var startTimeOnly = TimeOnly.FromDateTime(model.StartTime);
                    var endTime = model.StartTime.AddMinutes(movie.DurationMinutes.Value);
                    var endTimeOnly = TimeOnly.FromDateTime(endTime);

                    var minStartTime = branch.OpenHour.AddMinutes(15);

                    if (startTimeOnly < minStartTime)
                    {
                        ModelState.AddModelError("StartTime",
                            $"Giờ bắt đầu phải từ {minStartTime:HH\\:mm} trở đi (giờ mở cửa + 15 phút)!");
                    }

                    if (endTimeOnly > branch.CloseHour)
                    {
                        ModelState.AddModelError("StartTime",
                            $"Giờ kết thúc ({endTimeOnly:HH\\:mm}) vượt quá giờ đóng cửa ({branch.CloseHour:HH\\:mm})!");
                    }
                }
            }

            // ✅ VALIDATION 4: Kiểm tra xung đột lịch chiếu
            if (movie != null && movie.DurationMinutes.HasValue)
            {
                var endTime = model.StartTime.AddMinutes(movie.DurationMinutes.Value);
                var hasConflict = await _service.CheckTimeConflictAsync(
                    model.RoomID,
                    model.StartTime,
                    endTime
                );

                if (hasConflict)
                {
                    var conflicts = await _service.GetConflictingShowTimesAsync(
                        model.RoomID,
                        model.StartTime,
                        endTime
                    );

                    var conflictDetails = string.Join(", ", conflicts.Select(c =>
                        $"{c.Movie?.Title} ({c.StartTime:HH:mm})"));

                    ModelState.AddModelError("StartTime",
                        $"Thời gian chiếu bị trùng với các suất: {conflictDetails}");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(model);
            }

            var result = await _service.CreateAsync(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Thêm suất chiếu thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm suất chiếu!";
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
            ModelState.Remove("Tickets");

            // ✅ VALIDATION 1: Giờ bắt đầu phải > giờ hiện tại
            if (model.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Giờ bắt đầu phải lớn hơn giờ hiện tại!");
            }

            // ✅ VALIDATION 2: Giờ bắt đầu phải >= Ngày khởi chiếu của phim (Movie.StartTime)
            var movie = await _service.GetMovieByIdAsync(model.MovieID);
            if (movie != null && movie.StartTime.HasValue)
            {
                if (model.StartTime.Date < movie.StartTime.Value.Date)
                {
                    ModelState.AddModelError("StartTime",
                        $"Giờ bắt đầu phải từ ngày khởi chiếu của phim ({movie.StartTime.Value:dd/MM/yyyy}) trở đi!");
                }
            }

            // ✅ VALIDATION 3: Kiểm tra giờ hoạt động chi nhánh
            var room = await _service.GetRoomByIdAsync(model.RoomID);
            if (room?.Branch != null && movie != null && movie.DurationMinutes.HasValue)
            {
                var branch = room.Branch;

                if (branch.OpenHour != default && branch.CloseHour != default)
                {
                    var startTimeOnly = TimeOnly.FromDateTime(model.StartTime);
                    var endTime = model.StartTime.AddMinutes(movie.DurationMinutes.Value);
                    var endTimeOnly = TimeOnly.FromDateTime(endTime);
                    var minStartTime = branch.OpenHour.AddMinutes(15);

                    if (startTimeOnly < minStartTime)
                    {
                        ModelState.AddModelError("StartTime",
                            $"Giờ bắt đầu phải từ {minStartTime:HH\\:mm} trở đi!");
                    }

                    if (endTimeOnly > branch.CloseHour)
                    {
                        ModelState.AddModelError("StartTime",
                            $"Giờ kết thúc vượt quá giờ đóng cửa!");
                    }
                }
            }

            // ✅ VALIDATION 4: Kiểm tra xung đột (loại trừ chính nó)
            if (movie != null && movie.DurationMinutes.HasValue)
            {
                var endTime = model.StartTime.AddMinutes(movie.DurationMinutes.Value);
                var hasConflict = await _service.CheckTimeConflictAsync(
                    model.RoomID,
                    model.StartTime,
                    endTime,
                    model.ShowTimeID
                );

                if (hasConflict)
                {
                    var conflicts = await _service.GetConflictingShowTimesAsync(
                        model.RoomID,
                        model.StartTime,
                        endTime,
                        model.ShowTimeID
                    );

                    var conflictDetails = string.Join(", ", conflicts.Select(c =>
                        $"{c.Movie?.Title} ({c.StartTime:HH:mm})"));

                    ModelState.AddModelError("StartTime",
                        $"Thời gian chiếu bị trùng với các suất: {conflictDetails}");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await _service.UpdateAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật suất chiếu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Cập nhật thất bại!";
            }
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
            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result ? "Xóa suất chiếu thành công!" : "Xóa thất bại!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _service.RestoreAsync(id);
            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result ? "Khôi phục thành công!" : "Khôi phục thất bại!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetRoomsByBranch(string branchId)
        {
            var rooms = await _service.GetRoomsByBranchAsync(branchId);
            return Json(rooms.Select(r => new { value = r.RoomID, text = r.RoomName }));
        }

        [HttpGet]
        public async Task<JsonResult> GetMovieDuration(string movieId)
        {
            var movie = await _service.GetMovieByIdAsync(movieId);
            return Json(new
            {
                duration = movie?.DurationMinutes ?? 0,
                releaseDate = movie?.StartTime?.ToString("yyyy-MM-dd")
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetBranchHours(string branchId)
        {
            var branch = await _service.GetBranchByIdAsync(branchId);
            if (branch == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                openHour = branch.OpenHour.ToString("HH:mm"),
                closeHour = branch.CloseHour.ToString("HH:mm")
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetRoomTimeline(string roomId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return Json(new { success = false, message = "Ngày không hợp lệ" });

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
            catch
            {
                return Json(new { success = false, message = "Lỗi tải dữ liệu" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CheckTimeConflict(string roomId, DateTime startTime, int duration, string? excludeShowTimeId = null)
        {
            try
            {
                var endTime = startTime.AddMinutes(duration);
                var hasConflict = await _service.CheckTimeConflictAsync(roomId, startTime, endTime, excludeShowTimeId);
                return Json(new { success = true, hasConflict });
            }
            catch
            {
                return Json(new { success = false, hasConflict = true, message = "Lỗi kiểm tra xung đột" });
            }
        }

        private async Task LoadSelectLists()
        {
            var movies = await _service.GetAllMoviesAsync();
            var branches = await _service.GetAllBranchesAsync();
            ViewBag.Movies = new SelectList(movies, "MovieID", "Title");
            ViewBag.Branches = new SelectList(branches, "BranchID", "BranchName");
        }
    }
}