using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        public async Task<IActionResult> Index()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return View(movies);
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
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "movies");
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

            return "/images/movies/" + uniqueFileName;
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