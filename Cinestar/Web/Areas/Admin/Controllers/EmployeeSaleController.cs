using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeSaleController : Controller
    {
        private readonly IEmployeeService _employeeService;
        public EmployeeSaleController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        public async Task<IActionResult> Index()
        {
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out Guid employeeId))
            {
                TempData["Error"] = "Không tìm thấy thông tin nhân viên!";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Lấy thông tin employee từ database
            var employee = await _employeeService.GetEmployeeById(employeeId);

            if (employee == null)
            {
                TempData["Error"] = "Nhân viên không tồn tại!";
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            return View(employee);
        }
    }
}
