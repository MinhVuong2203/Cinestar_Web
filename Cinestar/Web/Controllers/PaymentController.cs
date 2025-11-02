using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
