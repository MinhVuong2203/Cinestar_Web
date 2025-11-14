using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, EmployeeMovies")]
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MoviesController(IMovieService movieService, IWebHostEnvironment webHostEnvironment)
        {
            _movieService = movieService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Movies
        
        public async Task<IActionResult> Index(int page = 1, string? search = null, string? filter = "all")
        {
            const int pageSize = 12; // Số phim mỗi trang

            var result = await _movieService.GetMoviesPagedAsync(page, pageSize, search, filter);

            return View(result);
        }

        // GET: Admin/Movies/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Admin/Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Upload image if provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    movie.ImageUrl = await UploadImageAsync(imageFile);
                }

                var result = await _movieService.CreateMovieAsync(movie);
                if (result)
                {
                    TempData["Success"] = "Thêm phim thành công!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Có lỗi xảy ra khi thêm phim!";
            }
            return View(movie);
        }

        // GET: Admin/Movies/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }
        public async Task<IActionResult> ChartMovie()
        {
            // Thống kê tổng quan
            var allMovies = await _movieService.GetAllMoviesAsync(); // Bạn cần thêm method này vào IMovieService
            var activeMovies = allMovies.Where(m => !m.IsDeleted).ToList();

            var totalMovies = activeMovies.Count;
            var nowShowing = activeMovies.Count(m => m.StartTime <= DateTime.Now && m.EndTime >= DateTime.Now);
            var comingSoon = activeMovies.Count(m => m.StartTime > DateTime.Now);
            var avgDuration = activeMovies.Where(m => m.DurationMinutes.HasValue)
                                          .Average(m => (double?)m.DurationMinutes) ?? 0;

            // Thống kê theo thể loại
            var moviesByGenre = activeMovies
                .Where(m => !string.IsNullOrEmpty(m.Genre))
                .SelectMany(m => m.Genre.Split(',').Select(g => g.Trim()))
                .GroupBy(g => g)
                .Select(g => new { Genre = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            // Thống kê theo độ tuổi
            var moviesByAge = activeMovies
                .Where(m => !string.IsNullOrEmpty(m.AgeLimit))
                .GroupBy(m => m.AgeLimit)
                .Select(g => new { AgeLimit = g.Key, Count = g.Count() })
                .OrderBy(x => x.AgeLimit)
                .ToList();

            // Thống kê theo ngôn ngữ
            var moviesByLanguage = activeMovies
                .Where(m => !string.IsNullOrEmpty(m.Language))
                .GroupBy(m => m.Language)
                .Select(g => new { Language = g.Key, Count = g.Count() })
                .ToList();

            // Thống kê theo thời lượng
            var durationGroups = activeMovies
                .Where(m => m.DurationMinutes.HasValue)
                .GroupBy(m => m.DurationMinutes < 90 ? "Ngắn (<90 phút)" :
                             m.DurationMinutes <= 120 ? "Trung bình (90-120 phút)" :
                             "Dài (>120 phút)")
                .Select(g => new { Group = g.Key, Count = g.Count() })
                .ToList();

            // Thống kê phim theo tháng (6 tháng gần nhất)
            var moviesByMonth = activeMovies
                .Where(m => m.StartTime.HasValue && m.StartTime.Value >= DateTime.Now.AddMonths(-6))
                .GroupBy(m => new { m.StartTime.Value.Year, m.StartTime.Value.Month })
                .Select(g => new {
                    Month = $"{g.Key.Month:00}/{g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Thống kê phụ đề và lồng tiếng
            var moviesWithSub = activeMovies.Count(m => !string.IsNullOrEmpty(m.Sub));
            var moviesWithDub = activeMovies.Count(m => m.Dub == true);
            var moviesWithoutSubDub = totalMovies - moviesWithSub - moviesWithDub;

            ViewBag.TotalMovies = totalMovies;
            ViewBag.NowShowing = nowShowing;
            ViewBag.ComingSoon = comingSoon;
            ViewBag.AvgDuration = Math.Round(avgDuration, 0);

            ViewBag.MoviesByGenre = System.Text.Json.JsonSerializer.Serialize(moviesByGenre);
            ViewBag.MoviesByAge = System.Text.Json.JsonSerializer.Serialize(moviesByAge);
            ViewBag.MoviesByLanguage = System.Text.Json.JsonSerializer.Serialize(moviesByLanguage);
            ViewBag.DurationGroups = System.Text.Json.JsonSerializer.Serialize(durationGroups);
            ViewBag.MoviesByMonth = System.Text.Json.JsonSerializer.Serialize(moviesByMonth);
            ViewBag.SubDubStats = System.Text.Json.JsonSerializer.Serialize(new[] {
        new { Type = "Có phụ đề", Count = moviesWithSub },
        new { Type = "Lồng tiếng", Count = moviesWithDub },
        new { Type = "Không có", Count = moviesWithoutSubDub }
    });

            return View();
        }
        // POST: Admin/Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Movie movie, IFormFile? imageFile)
        {
            if (id != movie.MovieID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Upload new image if provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(movie.ImageUrl))
                    {
                        DeleteImage(movie.ImageUrl);
                    }
                    movie.ImageUrl = await UploadImageAsync(imageFile);
                }

                var result = await _movieService.UpdateMovieAsync(movie);
                if (result)
                {
                    TempData["Success"] = "Cập nhật phim thành công!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Có lỗi xảy ra khi cập nhật phim!";
            }
            return View(movie);
        }

        // GET: Admin/Movies/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Admin/Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _movieService.SoftDeleteMovieAsync(id);
            if (result)
            {
                TempData["Success"] = "Xóa phim thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa phim!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Movies/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _movieService.RestoreMovieAsync(id);
            if (result)
            {
                TempData["Success"] = "Khôi phục phim thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi khôi phục phim!";
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<string> UploadImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image", "movie");
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

            return "/image/movie/" + uniqueFileName;
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