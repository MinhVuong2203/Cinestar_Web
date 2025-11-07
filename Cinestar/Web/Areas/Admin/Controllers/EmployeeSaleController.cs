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

        //Lấy danh sách suất chiếu theo movieId và ngày - FIXED VERSION
        [HttpGet]
        public async Task<JsonResult> GetShowTimes()
        {
            try
            {
                var movieId = Request.Query["movieId"].ToString();
                var date = Request.Query["date"].ToString();

                Console.WriteLine($"=== GetShowTimes Called ===");
                Console.WriteLine($"MovieId: '{movieId}'");
                Console.WriteLine($"Date: '{date}'");
                Console.WriteLine($"Full URL: {Request.Path}{Request.QueryString}");


                // 1. Validate parameters
                if (string.IsNullOrEmpty(movieId))
                {
                    Console.WriteLine("ERROR: MovieId is null or empty!");
                    return Json(new { success = false, message = "MovieId không được để trống!" });
                }

                if (string.IsNullOrEmpty(date))
                {
                    Console.WriteLine("ERROR: Date is null or empty!");
                    return Json(new { success = false, message = "Ngày không được để trống!" });
                }

                // 2. Validate Employee
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

                Console.WriteLine($"Employee BranchID: '{employee.BranchID}'");

                // 3. Parse date
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                {
                    Console.WriteLine($"ERROR: Failed to parse date '{date}'");
                    return Json(new { success = false, message = "Định dạng ngày không hợp lệ!" });
                }

                Console.WriteLine($"Parsed date: {selectedDate:yyyy-MM-dd}");

                // 4. Get showtimes
                Console.WriteLine($"Calling GetShowTimesByMovieAndDate('{movieId}', '{employee.BranchID}', '{selectedDate:yyyy-MM-dd}')");

                var showTimes = _employeeService.GetShowTimesByMovieAndDate(movieId, employee.BranchID, selectedDate);

                if (showTimes == null)
                {
                    Console.WriteLine("WARNING: Service returned null");
                    return Json(new { success = true, data = new List<object>() });
                }

                var showTimesList = showTimes.ToList();
                Console.WriteLine($"Found {showTimesList.Count} showtimes");

                // Log chi tiết từng showtime để debug
                foreach (var st in showTimesList)
                {
                    Console.WriteLine($"  - ShowTimeID: {st.ShowTimeID}, StartTime: {st.StartTime}, Room: {st.RoomName}");
                }

                return Json(new { success = true, data = showTimesList });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in GetShowTimes:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}
