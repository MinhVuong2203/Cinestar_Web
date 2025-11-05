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
            return View(new ShowTime { IsDeleted = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowTime model)
        {
            System.Diagnostics.Debug.WriteLine("========== DEBUG CREATE SHOWTIME ==========");
            System.Diagnostics.Debug.WriteLine($"ShowTimeID: '{model.ShowTimeID}'");
            System.Diagnostics.Debug.WriteLine($"MovieID: '{model.MovieID}'");
            System.Diagnostics.Debug.WriteLine($"RoomID: '{model.RoomID}'");
            System.Diagnostics.Debug.WriteLine($"StartTime: {model.StartTime}");
            System.Diagnostics.Debug.WriteLine($"Price: {model.Price}");
            System.Diagnostics.Debug.WriteLine($"IsDeleted: {model.IsDeleted}");

            model.IsDeleted = false;

            ModelState.Remove("ShowTimeID");
            ModelState.Remove("Movie");
            ModelState.Remove("Room");
            ModelState.Remove("Tickets");

            System.Diagnostics.Debug.WriteLine($"\nModelState.IsValid: {ModelState.IsValid}");
            System.Diagnostics.Debug.WriteLine($"ModelState.ErrorCount: {ModelState.ErrorCount}");

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("\n❌❌❌ MODELSTATE ERRORS ❌❌❌");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"\n*** Field: {state.Key} ***");
                        System.Diagnostics.Debug.WriteLine($"    AttemptedValue: '{state.Value.AttemptedValue}'");
                        System.Diagnostics.Debug.WriteLine($"    RawValue: '{state.Value.RawValue}'");
                        foreach (var error in state.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"    - Error: {error.ErrorMessage}");
                            if (error.Exception != null)
                                System.Diagnostics.Debug.WriteLine($"    - Exception: {error.Exception.Message}");
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("==========================================\n");
                await LoadSelectLists();
                return View(model);
            }

            System.Diagnostics.Debug.WriteLine("\n✅ ModelState is VALID, calling service...");
            var result = await _service.CreateAsync(model);
            System.Diagnostics.Debug.WriteLine($"Service returned: {result}");
            System.Diagnostics.Debug.WriteLine("==========================================\n");

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
            return Json(new { duration = movie?.DurationMinutes ?? 0 });
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
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
            catch (Exception ex)
            {
                return Json(new { success = false, hasConflict = true, message = ex.Message });
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
