using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    public class HomeController : Controller
    {
        //[Authorize(Roles = "Admin, EmployeeSales")]
       
        public IActionResult Index()
        {
            return View();
        }
    }
}
