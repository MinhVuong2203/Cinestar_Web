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
        

        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }
        [Authorize(Roles = "Admin, EmployeeSales, EmployeeMovies, EmployeeTechnician")]
        public async Task<IActionResult> IndexAsync()
        {
            try
            {
                ViewBag.TotalMovies = _homeService.GetTotalMovies();
                ViewBag.NowShowing = _homeService.GetNowShowingMovies();
                ViewBag.ComingSoon = _homeService.GetComingSoon();
                ViewBag.AvgDuration = _homeService.GetAverageDuration();
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

                var revenue = await _homeService.GetMonthlyRevenue(startOfMonth, endOfMonth);
                ViewBag.MonthlyRevenue = revenue.ToString("N0");

                var growth = await _homeService.GetRevenueGrowthPercentage(startOfMonth, endOfMonth);
                ViewBag.RevenueGrowth = growth;
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
