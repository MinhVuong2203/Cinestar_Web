using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Models;
using Web.Service;

namespace Web.Controllers
{
    public class ShowtimesController : Controller
    {
        public ICinemaBranchService _cinemaBranchService { get; set; }

        public ShowtimesController(ICinemaBranchService cinemaBranchService)
        {
            _cinemaBranchService = cinemaBranchService;
        }

        [LoadCinemaBranches]
        public IActionResult Index()
        {
            
            return View();
        }
    }
}
