using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Filters;
using Web.Models;
using Web.Service;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ICinemaBranchService _cinemaBranchService { get; set; }
        private readonly IMovieService_Cus _movieService_Cus;


        public HomeController(ICinemaBranchService cinemaBranchService, IMovieService_Cus movieService_Cus)
        {
            _cinemaBranchService = cinemaBranchService;
            _movieService_Cus = movieService_Cus;

        }

        [LoadCinemaBranches]
        public IActionResult Index()
        {
            Debug.WriteLine("=== HomeController.Index START ===");

            // Lấy danh sách phim đang chiếu và sắp chiếu
            var nowShowing = _movieService_Cus.GetNowShowingMoviesAsync(12).Result;
            var comingSoon = _movieService_Cus.GetComingSoonMoviesAsync(12).Result;

            // DEBUG: Kiểm tra kết quả
            Debug.WriteLine($"HomeController - Now Showing: {nowShowing?.Count ?? 0} movies");
            Debug.WriteLine($"HomeController - Coming Soon: {comingSoon?.Count ?? 0} movies");

            // Truyền dữ liệu qua ViewData
            ViewData["NowShowing"] = nowShowing;
            ViewData["ComingSoon"] = comingSoon;

            Debug.WriteLine("=== HomeController.Index END ===");
            return View();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
