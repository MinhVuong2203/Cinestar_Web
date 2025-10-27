using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ShowtimesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
