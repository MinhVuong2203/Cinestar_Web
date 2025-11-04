using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Service;

namespace Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly ICinemaBranchService _cinemaBranchService;
        public MovieController(ICinemaBranchService cinemaBranchService)
        {
            _cinemaBranchService = cinemaBranchService;
        }
        [LoadCinemaBranches]
        public IActionResult Index()
        {
            // Lấy danh sách các thành phố có rạp
            var cities = _cinemaBranchService.GetListCityBranches();
            ViewData["lstCity"] = cities;
            return View();
        }
    }
}
