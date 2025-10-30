using Microsoft.AspNetCore.Mvc;
using Web.Filters;

namespace Web.Controllers
{
    public class MovieController : Controller
    {
        [LoadCinemaBranches]
        public IActionResult Index()
        {
            return View();
        }
    }
}
