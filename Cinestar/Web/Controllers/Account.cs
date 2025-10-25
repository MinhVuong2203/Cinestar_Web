using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class Account : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
