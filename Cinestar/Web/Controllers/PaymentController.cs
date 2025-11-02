using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PaymentMethod()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PaymentMethod(string fullname, string phone, string email)
        {
            // Server-side validation
            if (string.IsNullOrWhiteSpace(fullname) || 
                string.IsNullOrWhiteSpace(phone) || 
                string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng. Vui lòng nhập lại.";
                return RedirectToAction("Index");
            }

            // Store customer info in TempData or Session for the next view
            TempData["CustomerInfo"] = JsonConvert.SerializeObject(new
            {
                fullname = fullname,
                phone = phone,
                email = email,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return View();
        }
    }
}
