using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Admin.Service;
using Web.Data;
using Web.Service;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeSaleController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IProductService _productService;
        private readonly CineStarContext _context;
        private readonly IMovieService_Cus _movieService_Cus;
        public EmployeeSaleController(IEmployeeService employeeService, IProductService productService, 
            CineStarContext context, IMovieService_Cus movieService_Cus)
        {
            _employeeService = employeeService;
            _productService = productService;
            _context = context;
            _movieService_Cus = movieService_Cus;
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
            
            //var ticketTypes = _employeeService.GetTicketTypesAndPrices(movieId, employee.BranchID);
            //ViewData["TicketTypes"] = ticketTypes;

            //lấy danh sách sản phẩm
            var lstProducts = _productService.GetAllProduct();
            ViewData["lstProducts"] = lstProducts;


            return View(employee);
        }

        //Hàm để lấy ticket types theo showTimeId
        [HttpGet]
        public IActionResult GetTicketTypesByShowTime(string showTimeId, string movieId, string branchId)
        {
            try
            {
                Console.WriteLine($"=== GetTicketTypesByShowTime Controller ===");
                Console.WriteLine($"showTimeId: {showTimeId}, movieId: {movieId}, branchId: {branchId}");

                if (string.IsNullOrEmpty(showTimeId) || string.IsNullOrEmpty(movieId) || string.IsNullOrEmpty(branchId))
                {
                    Console.WriteLine("ERROR: Missing parameters");
                    return Json(new { success = false, message = "Missing required parameters" });
                }

                var ticketTypes = _employeeService.GetTicketTypesAndPrices(movieId, branchId, showTimeId);

                if (ticketTypes == null)
                {
                    Console.WriteLine("ERROR: ticketTypes is null from service");
                    return Json(new { success = false, message = "Không tìm thấy thông tin vé" });
                }

                // ✅ Convert dynamic to concrete object để serialize đúng
                var response = new
                {
                    success = true,
                    ticketTypes = new
                    {
                        Standard = ticketTypes.Standard != null ? new
                        {
                            Name = (string)ticketTypes.Standard.Name,
                            Description = (string)ticketTypes.Standard.Description,
                            Price = (decimal)ticketTypes.Standard.Price,
                            AvailableCount = (int)ticketTypes.Standard.AvailableCount,
                            Icon = (string)ticketTypes.Standard.Icon
                        } : null,
                        VIP = ticketTypes.VIP != null ? new
                        {
                            Name = (string)ticketTypes.VIP.Name,
                            Description = (string)ticketTypes.VIP.Description,
                            Price = (decimal)ticketTypes.VIP.Price,
                            AvailableCount = (int)ticketTypes.VIP.AvailableCount,
                            Icon = (string)ticketTypes.VIP.Icon
                        } : null,
                        Couple = ticketTypes.Couple != null ? new
                        {
                            Name = (string)ticketTypes.Couple.Name,
                            Description = (string)ticketTypes.Couple.Description,
                            Price = (decimal)ticketTypes.Couple.Price,
                            AvailableCount = (int)ticketTypes.Couple.AvailableCount,
                            Icon = (string)ticketTypes.Couple.Icon
                        } : null
                    }
                };

                Console.WriteLine($"SUCCESS: Returning ticket types to client");
                Console.WriteLine($"Response JSON: {System.Text.Json.JsonSerializer.Serialize(response)}");

                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in GetTicketTypesByShowTime: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        //Hàm lấy tên phòng chiếu
        [HttpPost]
        public IActionResult GetRoomNameByMovieShowTimeDate(string movieId, string showTimeId)
        {
            try
            {
                var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out Guid employeeId))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                var employee = _employeeService.GetEmployeeById(employeeId).Result;
                if (employee == null)
                {
                    return Json(new { success = false, message = "Nhân viên không tồn tại" });
                }

                // Lấy thông tin showtime để có StartTime
                var showTime = _context.ShowTimes
                    .Include(st => st.Room)
                    .FirstOrDefault(st => st.ShowTimeID == showTimeId && !st.IsDeleted);

                if (showTime == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy suất chiếu" });
                }

                var roomInfo = _employeeService.GetRoomNameByMovieShowTimeDate(
                    movieId,
                    employee.BranchID,
                    showTime.StartTime,
                    showTime.StartTime.ToString("HH:mm")
                );

                if (roomInfo != null)
                {
                    return Json(new
                    {
                        success = true,
                        roomName = roomInfo.RoomName,
                        roomType = roomInfo.RoomType
                    });
                }

                return Json(new { success = false, message = "Không tìm thấy phòng chiếu" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //lấy sơ đồ chỗ ngồi
        public async Task<IActionResult> GetSeatingLayout(string showTimeId)
        {
            try
            {
                if (string.IsNullOrEmpty(showTimeId))
                {
                    return Json(new { success = false, message = "ShowTimeId is required" });
                }

                // Lấy thông tin showtime
                var showTime = _context.ShowTimes
                    .Include(st => st.Room)
                    .FirstOrDefault(st => st.ShowTimeID == showTimeId && !st.IsDeleted);

                if (showTime == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy suất chiếu" });
                }

                // Lấy danh sách ghế của phòng
                var seats = _context.Seats
                    .Where(s => s.RoomID == showTime.RoomID && !s.IsDeleted)
                    .OrderBy(s => s.SeatName)
                    .Select(s => new
                    {
                        seatId = s.SeatID,
                        seatName = s.SeatName,
                        seatType = s.SeatType,
                        status = "Trống" // Mặc định
                    })
                    .ToList();

                // Lấy danh sách vé đã đặt cho suất chiếu này
                var bookedTickets = _context.Tickets
                    .Where(t => t.ShowTimeID == showTimeId &&
                               !t.IsDeleted &&
                               (t.Status == "Đã đặt" || t.Status == "Đã thanh toán"))
                    .Select(t => t.SeatID)
                    .ToList();

                // Cập nhật status cho ghế đã đặt
                var seatsWithStatus = seats.Select(s => new
                {
                    s.seatId,
                    s.seatName,
                    s.seatType,
                    status = bookedTickets.Contains(s.seatId) ? "Đã đặt" : "Trống"
                }).ToList();

                return Json(new { success = true, seats = seatsWithStatus });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSeatingLayout: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
