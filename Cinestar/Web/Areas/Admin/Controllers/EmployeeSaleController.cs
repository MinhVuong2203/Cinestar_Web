using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeSaleController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IProductService _productService;
        public EmployeeSaleController(IEmployeeService employeeService, IProductService productService)
        {
            _employeeService = employeeService;
            _productService = productService;
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

        //chọn phim để bán vé
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

            if (employee == null)
            {
                TempData["Error"] = "Nhân viên không tồn tại!";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            //lấy danh sách phim theo chi nhánh của nhân viên
            var lstMovies = _employeeService.GetMoviesByEmployeeBranchId(employee.BranchID);
            ViewData["lstMovies"] = lstMovies;

            // CHỈ set TempData error nếu KHÔNG có phim
            // KHÔNG set nếu có phim để tránh thông báo lỗi khi có phim
            if (lstMovies == null || !lstMovies.Any())
            {
                ViewBag.NoMoviesMessage = "Không có phim nào đang chiếu tại chi nhánh này!";
            }

            return View(employee);
        }

        //trang bán vé
        [HttpGet("TicketSelling/{movieId}")]
        public async Task<IActionResult> TicketSelling(string movieId)
        {
            // Log để debug
            Console.WriteLine($"=== TicketSelling Action Called ===");
            Console.WriteLine($"MovieId parameter: '{movieId}'");
            Console.WriteLine($"Query string: {Request.QueryString}");
            
            // Thử đọc từ query string nếu parameter null
            if (string.IsNullOrEmpty(movieId))
            {
                movieId = Request.Query["movieId"].ToString();
                Console.WriteLine($"MovieId from query string: '{movieId}'");
            }
            
            // Thử cả MovieId với chữ M hoa
            if (string.IsNullOrEmpty(movieId))
            {
                movieId = Request.Query["MovieId"].ToString();
                Console.WriteLine($"MovieId from query string (capitalized): '{movieId}'");
            }

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


            // Validate movieId
            if (string.IsNullOrEmpty(movieId))
            {
                Console.WriteLine("MovieId is still null or empty after all attempts!");
                TempData["Error"] = "Vui lòng chọn phim!";
                return RedirectToAction("SaleTicket");
            }

            Console.WriteLine($"Final movieId: '{movieId}'");
          
            // Pass movieId to ViewData
            ViewData["MovieId"] = movieId;

            var showTime = _employeeService.GetShowTimesByMovieAndDate(movieId, employee.BranchID, DateTime.Today);
            ViewData["ShowTimes"] = showTime;
            
            var ticketTypes = _employeeService.GetTicketTypesAndPrices(movieId, employee.BranchID);
            ViewData["TicketTypes"] = ticketTypes;

            //lấy danh sách sản phẩm
            var lstProducts = _productService.GetAllProduct();
            ViewData["lstProducts"] = lstProducts;

            return View(employee);
        }

    }
}
