using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Service;

namespace Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly ICinemaBranchService _cinemaBranchService;
        private readonly IMovieService_Cus _movieService_Cus;
        public MovieController(ICinemaBranchService cinemaBranchService, IMovieService_Cus movieService_Cus)
        {
            _cinemaBranchService = cinemaBranchService;
            _movieService_Cus = movieService_Cus;
        }
        [LoadCinemaBranches]
        public IActionResult Index()
        {
            // Lấy danh sách các thành phố có rạp
            var cities = _cinemaBranchService.GetListCityBranches();
            ViewData["lstCity"] = cities;
            // Lấy danh sách phim đang chiếu và sắp chiếu
            var nowShowing = _movieService_Cus.GetNowShowingMoviesAsync(12).Result;
            var comingSoon = _movieService_Cus.GetComingSoonMoviesAsync(12).Result;
            // Truyền dữ liệu qua ViewData
            ViewData["NowShowing"] = nowShowing;
            ViewData["ComingSoon"] = comingSoon;
            return View();
        }
 
        public IActionResult Details(string id)
        {
            // Lấy danh sách các thành phố có rạp
            var cities = _cinemaBranchService.GetListCityBranches();
            ViewData["lstCity"] = cities;
            return View();
        }


    }
}