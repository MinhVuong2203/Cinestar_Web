using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Areas.Admin.Service;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        //[Authorize(Roles = "Admin, EmployeeSales, ")]

        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }
        public IActionResult Index()
        {
            try
            {
                ViewBag.TotalMovies = _homeService.GetTotalMovies();
                ViewBag.NowShowing = _homeService.GetNowShowingMovies();
                ViewBag.ComingSoon = _homeService.GetComingSoon();
                ViewBag.AvgDuration = _homeService.GetAverageDuration();

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu thống kê!";
                ViewBag.TotalMovies = 0;
                ViewBag.NowShowing = 0;
                ViewBag.ComingSoon = 0;
                ViewBag.AvgDuration = 0;
                return View();
            }
        }
    }
}
