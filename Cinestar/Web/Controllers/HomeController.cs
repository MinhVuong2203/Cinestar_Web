using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Models;
using Web.Service;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ICinemaBranchService _cinemaBranchService { get; set; }

        public HomeController(ICinemaBranchService cinemaBranchService)
        {
            _cinemaBranchService = cinemaBranchService;
        }

        public IActionResult Index()
        {
            List<CinemaBranch> list = _cinemaBranchService.GetBranches();
            return View(list);
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
