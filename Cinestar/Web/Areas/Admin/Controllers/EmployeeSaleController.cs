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

        public async Task<IActionResult> SaleTicket()
        {
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out Guid employeeId))
            {
                TempData["Error"] = "Không tìm thấy thông tin nhân viên!";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Lấy thông tin employee từ database
            var employee = await _employeeService.GetEmployeeById(employeeId);

            //lấy danh sách phim theo chi nhánh của nhân viên
            ViewData["lstMovies"] = _employeeService.GetMoviesByEmployeeBranchId(employee.BranchID);
            if (employee == null)
            {
                TempData["Error"] = "Nhân viên không tồn tại!";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            return View(employee);
        }

        //lấy loại vé và giá theo phim
        [HttpGet]
        public async Task<JsonResult> GetTicketTypes(string movieId)
        {
            try
            {
                var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out Guid employeeId))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên!" });
                }

                var employee = await _employeeService.GetEmployeeById(employeeId);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Nhân viên không tồn tại!" });
                }

                var ticketTypes = _employeeService.GetTicketTypesAndPrices(movieId, employee.BranchID);

                if (ticketTypes == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin vé cho phim này!" });
                }

                return Json(new { success = true, data = ticketTypes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}
