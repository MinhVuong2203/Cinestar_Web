using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Admin.Service;
using Web.Data;
using Web.Models;
using Web.Models.DTOs;
using Web.Service;
using IProductService = Web.Areas.Admin.Service.IProductService;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeSaleController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IProductService _productService;
        private readonly CineStarContext _context;
        private readonly IMovieService_Cus _movieService_Cus;
        private readonly IPayOsService _payOsService;
        public EmployeeSaleController(IEmployeeService employeeService, IProductService productService, 
            CineStarContext context, IMovieService_Cus movieService_Cus, IPayOsService payOsService)
        {
            _employeeService = employeeService;
            _productService = productService;
            _context = context;
            _movieService_Cus = movieService_Cus;
            _payOsService = payOsService;
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

        //Thông tin nhân viên
        public async Task<IActionResult> EmployeeInfo()
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

        //trang bán bắp nước
        public async Task<IActionResult> SaleProduct()
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
            //lấy danh sách sản phẩm theo chi nhánh của nhân viên
            var lstProducts = _productService.GetAllProduct();
            ViewData["lstProducts"] = lstProducts;
            // CHỈ set TempData error nếu KHÔNG có sản phẩm
            // KHÔNG set nếu có sản phẩm để tránh thông báo lỗi khi có sản phẩm
            if (lstProducts == null || !lstProducts.Any())
            {
                ViewBag.NoProductsMessage = "Không có sản phẩm nào tại chi nhánh này!";
            }
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

        [HttpGet]
        public async Task<IActionResult> CheckCustomerByPhone(string phone)
        {
            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Phone == phone && !c.IsDeleted);

                if (customer == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy khách hàng với số điện thoại này"
                    });
                }

                return Json(new
                {
                    success = true,
                    customer = new
                    {
                        customerId = customer.CustomerID,
                        fullName = customer.FullName,
                        email = customer.Email,
                        phone = customer.Phone,
                        point = customer.Point ?? 0,
                        vipLevel = customer.VipLevel ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ShowTimeId))
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(employeeIdClaim, out Guid employeeId))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                var employee = await _employeeService.GetEmployeeById(employeeId);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Nhân viên không tồn tại" });
                }

                // Validate showtime
                var showTime = await _context.ShowTimes
                    .Include(st => st.Movie)
                    .Include(st => st.Room)
                    .ThenInclude(r => r.Branch)
                    .FirstOrDefaultAsync(st => st.ShowTimeID == request.ShowTimeId && !st.IsDeleted);

                if (showTime == null)
                {
                    return Json(new { success = false, message = "Suất chiếu không tồn tại" });
                }

                // ✅ XỬ LÝ CustomerId: Convert string sang Guid? hoặc để null
                Guid? customerGuid = null;

                if (!request.IsGuest && !string.IsNullOrEmpty(request.CustomerId))
                {
                    // Nếu không phải guest và có customerId
                    if (Guid.TryParse(request.CustomerId, out Guid parsedGuid))
                    {
                        customerGuid = parsedGuid;
                    }
                    else
                    {
                        // Nếu không parse được thành Guid (VD: "VL-..."), coi như guest
                        request.IsGuest = true;
                        customerGuid = null;
                    }
                }

                // ✅ 1. Tạo Invoice
                var invoice = new Invoice
                {
                    InvoiceID = Guid.NewGuid(),
                    EmployeeID = employeeId,
                    CustomerID = customerGuid, // ✅ Sử dụng Guid? đã xử lý
                    BranchID = employee.BranchID,
                    IssueDate = DateTime.Now,
                    TotalAmount = request.TotalAmount,
                    Discount = 0,
                    Status = "Chờ thanh toán",
                    IsDeleted = false
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // ✅ 2. Lock tickets và tạo InvoiceTicket
                foreach (var seatItem in request.Seats)
                {
                    var ticket = await _context.Tickets
                        .FirstOrDefaultAsync(t =>
                            t.ShowTimeID == request.ShowTimeId &&
                            t.SeatID == seatItem.SeatId &&
                            !t.IsDeleted);

                    if (ticket == null)
                    {
                        _context.Invoices.Remove(invoice);
                        await _context.SaveChangesAsync();
                        return Json(new { success = false, message = $"Không tìm thấy vé cho ghế {seatItem.SeatName}" });
                    }

                    if (ticket.Status != "Trống")
                    {
                        _context.Invoices.Remove(invoice);
                        await _context.SaveChangesAsync();
                        return Json(new { success = false, message = $"Ghế {seatItem.SeatName} đã được đặt" });
                    }

                    // Lock ticket
                    ticket.Status = "Đã đặt";
                    ticket.LockedBy = customerGuid; // ✅ Sử dụng Guid? đã xử lý
                    ticket.LockedAt = DateTime.Now;

                    var invoiceTicket = new InvoiceTicket
                    {
                        InvoiceID = invoice.InvoiceID,
                        TicketID = ticket.TicketID,
                        Quantity = 1,
                        UnitPrice = ticket.Price ?? 0
                    };

                    _context.InvoiceTickets.Add(invoiceTicket);
                }

                // ✅ 3. Tạo InvoiceProduct
                if (request.Products != null && request.Products.Any())
                {
                    foreach (var product in request.Products)
                    {
                        var invoiceProduct = new InvoiceProduct
                        {
                            InvoiceID = invoice.InvoiceID,
                            ProductID = product.ProductId,
                            Quantity = product.Quantity,
                            UnitPrice = product.Price
                        };

                        _context.InvoiceProducts.Add(invoiceProduct);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đặt vé thành công",
                    invoiceId = invoice.InvoiceID.ToString(),
                    movieTitle = showTime.Movie?.Title,
                    totalAmount = request.TotalAmount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductBooking([FromBody] BookingRequestDto request)
        {
            try
            {
                if (request == null || request.Products == null || !request.Products.Any())
                {
                    return Json(new { success = false, message = "Không có sản phẩm nào được chọn" });
                }

                var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(employeeIdClaim, out Guid employeeId))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                var employee = await _employeeService.GetEmployeeById(employeeId);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Nhân viên không tồn tại" });
                }

                // ✅ XỬ LÝ CustomerId
                Guid? customerGuid = null;

                if (!request.IsGuest && !string.IsNullOrEmpty(request.CustomerId))
                {
                    if (Guid.TryParse(request.CustomerId, out Guid parsedGuid))
                    {
                        customerGuid = parsedGuid;
                    }
                    else
                    {
                        request.IsGuest = true;
                        customerGuid = null;
                    }
                }

                // ✅ Tính tổng tiền từ products
                decimal totalAmount = 0;
                foreach (var product in request.Products)
                {
                    totalAmount += product.Price * product.Quantity;
                }

                // ✅ Đảm bảo totalAmount là integer (không có phần thập phân)
                totalAmount = Math.Floor(totalAmount);

                // ✅ 1. Tạo Invoice
                var invoice = new Invoice
                {
                    InvoiceID = Guid.NewGuid(),
                    EmployeeID = employeeId,
                    CustomerID = customerGuid,
                    BranchID = employee.BranchID,
                    IssueDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    Discount = 0,
                    Status = "Chờ thanh toán",
                    IsDeleted = false
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // ✅ 2. Tạo InvoiceProduct (KHÔNG CÓ InvoiceTicket)
                foreach (var product in request.Products)
                {
                    var invoiceProduct = new InvoiceProduct
                    {
                        InvoiceID = invoice.InvoiceID,
                        ProductID = product.ProductId,
                        Quantity = product.Quantity,
                        UnitPrice = product.Price
                    };

                    _context.InvoiceProducts.Add(invoiceProduct);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đặt hàng thành công",
                    invoiceId = invoice.InvoiceID.ToString(),
                    totalAmount = totalAmount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ✅ Trang chọn phương thức thanh toán
        [HttpGet]
        public async Task<IActionResult> PaymentMethod(string invoiceId)
        {
            if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
            {
                TempData["Error"] = "Không tìm thấy hóa đơn!";
                return RedirectToAction("Index");
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                        .ThenInclude(t => t.ShowTime)
                            .ThenInclude(st => st.Movie)
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                        .ThenInclude(t => t.Seat)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

            if (invoice == null)
            {
                TempData["Error"] = "Hóa đơn không tồn tại!";
                return RedirectToAction("Index");
            }

            // Check timeout (15 phút)
            if (invoice.IssueDate?.AddMinutes(15) < DateTime.Now)
            {
                await CancelInvoice(invoiceGuid);
                TempData["Error"] = "Đơn hàng đã hết hạn (quá 15 phút)!";
                return RedirectToAction("Index");
            }

            return View(invoice);
        }

        // ✅ Xử lý thanh toán tiền mặt
        [HttpPost]
        public async Task<IActionResult> ProcessCashPayment(string invoiceId)
        {
            try
            {
                if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
                {
                    return Json(new { success = false, message = "Invoice ID không hợp lệ" });
                }

                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceTickets)
                        .ThenInclude(it => it.Ticket)
                    .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

                if (invoice == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // ✅ Cập nhật invoice status
                invoice.Status = "Đã thanh toán";

                // ✅ Cập nhật ticket status
                foreach (var invoiceTicket in invoice.InvoiceTickets)
                {
                    invoiceTicket.Ticket.Status = "Đã thanh toán";
                }

                // ✅ Tạo payment record
                var payment = new Payment
                {
                    PaymentID = Guid.NewGuid(),
                    InvoiceID = invoice.InvoiceID,
                    Method = "Tiền mặt",
                    Amount = invoice.TotalAmount,
                    PaymentTime = DateTime.Now
                };

                _context.Payments.Add(payment);

                // ✅ Cộng điểm cho customer (nếu không phải guest)
                if (invoice.CustomerID != null)
                {
                    var customer = await _context.Customers.FindAsync(invoice.CustomerID);
                    if (customer != null)
                    {
                        var pointsToAdd = Math.Floor((Decimal)invoice.TotalAmount / 10000);
                        customer.Point = (customer.Point ?? 0) + pointsToAdd;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thanh toán thành công",
                    invoiceId = invoice.InvoiceID.ToString()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ✅ Xử lý thanh toán PayOS
        [HttpPost]
        public async Task<IActionResult> ProcessPayOsPayment(string invoiceId)
        {
            try
            {
                if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
                {
                    return Json(new { success = false, message = "Invoice ID không hợp lệ" });
                }

                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceTickets)
                        .ThenInclude(it => it.Ticket)
                            .ThenInclude(t => t.ShowTime)
                                .ThenInclude(st => st.Movie)
                    .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

                if (invoice == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                if (invoice.Status == "Đã thanh toán")
                {
                    return Json(new { success = false, message = "Hóa đơn đã được thanh toán" });
                }

                // ✅ Lấy thông tin khách hàng
                string buyerName = invoice.Customer?.FullName ?? "Khách vãng lai";
                string buyerEmail = invoice.Customer?.Email ?? "guest@cinestar.com";
                string buyerPhone = invoice.Customer?.Phone ?? "0000000000";

                // ✅ Tạo mô tả
                var movieTitle = invoice.InvoiceTickets?.FirstOrDefault()?.Ticket?.ShowTime?.Movie?.Title ?? "Vé xem phim";
                var description = $"Thanh toán vé xem phim - {movieTitle}";

                // ✅ Tạo payment link
                var paymentResult = await _payOsService.CreateTicketPaymentLink(
                    invoiceGuid,
                    invoice.TotalAmount.Value,
                    buyerName,
                    buyerEmail,
                    buyerPhone,
                    description,
                    isAdminSale: true
                );

                if (paymentResult == null)
                {
                    return Json(new { success = false, message = "Không thể tạo link thanh toán" });
                }

                // ✅ Lưu OrderCode vào Invoice (cần thêm property OrderCode vào Invoice model)
                // Tạm thời lưu vào một bảng riêng hoặc sử dụng field có sẵn

                return Json(new
                {
                    success = true,
                    message = "Tạo link thanh toán thành công",
                    paymentUrl = paymentResult.checkoutUrl,
                    orderCode = paymentResult.orderCode
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ✅ Callback sau khi thanh toán PayOS thành công
        [HttpGet]
        public async Task<IActionResult> PayOsSuccess(string invoiceId, long orderCode)
        {
            try
            {
                if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn!";
                    return RedirectToAction("Index");
                }

                // ✅ Kiểm tra trạng thái thanh toán từ PayOS
                var isSuccess = await _payOsService.IsPaymentSuccess(orderCode);

                if (!isSuccess)
                {
                    TempData["Error"] = "Thanh toán chưa được xác nhận!";
                    return RedirectToAction("PaymentMethod", new { invoiceId });
                }

                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceTickets)
                        .ThenInclude(it => it.Ticket)
                    .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

                if (invoice == null)
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn!";
                    return RedirectToAction("Index");
                }

                // ✅ Cập nhật invoice status
                invoice.Status = "Đã thanh toán";

                // ✅ Cập nhật ticket status
                foreach (var invoiceTicket in invoice.InvoiceTickets)
                {
                    invoiceTicket.Ticket.Status = "Đã thanh toán";
                }

                // ✅ Tạo payment record
                var payment = new Payment
                {
                    PaymentID = Guid.NewGuid(),
                    InvoiceID = invoice.InvoiceID,
                    Method = "PayOS",
                    Amount = invoice.TotalAmount,
                    PaymentTime = DateTime.Now
                };

                _context.Payments.Add(payment);

                // ✅ Cộng điểm cho customer (nếu không phải guest)
                if (invoice.CustomerID != null)
                {
                    var customer = await _context.Customers.FindAsync(invoice.CustomerID);
                    if (customer != null)
                    {
                        var pointsToAdd = Math.Floor(invoice.TotalAmount.Value / 10000);
                        customer.Point = (customer.Point ?? 0) + pointsToAdd;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Thanh toán thành công!";
                return RedirectToAction("PaymentSuccess", new { invoiceId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xác nhận thanh toán!";
                return RedirectToAction("Index");
            }
        }

        // ✅ Callback khi hủy thanh toán PayOS
        [HttpGet]
        public async Task<IActionResult> PayOsCancel(string? invoiceId, string? code, string? id, bool? cancel)
        {
            Console.WriteLine($"=== PayOsCancel Called ===");
            Console.WriteLine($"invoiceId: {invoiceId}");
            Console.WriteLine($"Full URL: {Request.Path}{Request.QueryString}");

            if (!string.IsNullOrEmpty(invoiceId) && Guid.TryParse(invoiceId, out Guid invoiceGuid))
            {
                await CancelInvoice(invoiceGuid);
            }

            // ✅ Option 1: Hiển thị view
            return View("PaymentCancel");

            // ✅ Option 2: Redirect trực tiếp về Index với thông báo
            // TempData["Warning"] = "Thanh toán đã bị hủy!";
            // return RedirectToAction("Index");
        }

        // ✅ Trang hiển thị thành công
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string invoiceId)
        {
            if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
            {
                TempData["Error"] = "Không tìm thấy hóa đơn!";
                return RedirectToAction("Index");
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                        .ThenInclude(t => t.ShowTime)
                            .ThenInclude(st => st.Movie)
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                        .ThenInclude(t => t.Seat)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

            if (invoice == null)
            {
                TempData["Error"] = "Hóa đơn không tồn tại!";
                return RedirectToAction("Index");
            }

            return View(invoice);
        }

        // ✅ Hủy invoice (khi timeout hoặc cancel)
        private async Task CancelInvoice(Guid invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceId);

            if (invoice != null)
            {
                invoice.Status = "Đã hủy";

                foreach (var invoiceTicket in invoice.InvoiceTickets)
                {
                    invoiceTicket.Ticket.Status = "Trống";
                    invoiceTicket.Ticket.LockedBy = null;
                    invoiceTicket.Ticket.LockedAt = null;
                }

                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProductPaymentMethod(string invoiceId)
        {
            if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
            {
                TempData["Error"] = "Không tìm thấy hóa đơn!";
                return RedirectToAction("SaleProduct");
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

            if (invoice == null)
            {
                TempData["Error"] = "Hóa đơn không tồn tại!";
                return RedirectToAction("SaleProduct");
            }

            // Check timeout (15 phút)
            if (invoice.IssueDate?.AddMinutes(15) < DateTime.Now)
            {
                await CancelProductInvoice(invoiceGuid);
                TempData["Error"] = "Đơn hàng đã hết hạn (quá 15 phút)!";
                return RedirectToAction("SaleProduct");
            }

            return View(invoice);
        }

        // ✅ Hủy invoice cho product (không cần unlock tickets)
        private async Task CancelProductInvoice(Guid invoiceId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceId);

            if (invoice != null)
            {
                invoice.Status = "Đã hủy";
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Trang hiển thị thành công cho Product
        [HttpGet]
        public async Task<IActionResult> ProductPaymentSuccess(string invoiceId)
        {
            if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
            {
                TempData["Error"] = "Không tìm thấy hóa đơn!";
                return RedirectToAction("SaleProduct");
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

            if (invoice == null)
            {
                TempData["Error"] = "Hóa đơn không tồn tại!";
                return RedirectToAction("SaleProduct");
            }

            return View(invoice);
        }

        // ✅ Callback sau khi thanh toán PayOS thành công cho Product
        [HttpGet]
        public async Task<IActionResult> ProductPayOsSuccess(string invoiceId, long orderCode)
        {
            try
            {
                if (!Guid.TryParse(invoiceId, out Guid invoiceGuid))
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn!";
                    return RedirectToAction("SaleProduct");
                }

                // ✅ Kiểm tra trạng thái thanh toán từ PayOS
                var isSuccess = await _payOsService.IsPaymentSuccess(orderCode);

                if (!isSuccess)
                {
                    TempData["Error"] = "Thanh toán chưa được xác nhận!";
                    return RedirectToAction("ProductPaymentMethod", new { invoiceId });
                }

                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

                if (invoice == null)
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn!";
                    return RedirectToAction("SaleProduct");
                }

                // ✅ Cập nhật invoice status
                invoice.Status = "Đã thanh toán";

                // ✅ Tạo payment record
                var payment = new Payment
                {
                    PaymentID = Guid.NewGuid(),
                    InvoiceID = invoice.InvoiceID,
                    Method = "PayOS",
                    Amount = invoice.TotalAmount,
                    PaymentTime = DateTime.Now
                };

                _context.Payments.Add(payment);

                // ✅ Cộng điểm cho customer (nếu không phải guest)
                if (invoice.CustomerID != null)
                {
                    var customer = await _context.Customers.FindAsync(invoice.CustomerID);
                    if (customer != null)
                    {
                        var pointsToAdd = Math.Floor(invoice.TotalAmount.Value / 10000);
                        customer.Point = (customer.Point ?? 0) + pointsToAdd;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Thanh toán thành công!";
                return RedirectToAction("ProductPaymentSuccess", new { invoiceId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xác nhận thanh toán!";
                return RedirectToAction("SaleProduct");
            }
        }

        // ✅ Xử lý thanh toán tiền mặt cho Product
        [HttpPost]
        public async Task<IActionResult> ProcessProductCashPayment([FromBody] PaymentRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.InvoiceId) || !Guid.TryParse(request.InvoiceId, out Guid invoiceGuid))
                {
                    return Json(new { success = false, message = "Invoice ID không hợp lệ" });
                }

                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

                if (invoice == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // ✅ Cập nhật invoice status
                invoice.Status = "Đã thanh toán";

                // ✅ Tạo payment record
                var payment = new Payment
                {
                    PaymentID = Guid.NewGuid(),
                    InvoiceID = invoice.InvoiceID,
                    Method = "Tiền mặt",
                    Amount = invoice.TotalAmount,
                    PaymentTime = DateTime.Now
                };

                _context.Payments.Add(payment);

                // ✅ Cộng điểm cho customer (nếu không phải guest)
                if (invoice.CustomerID != null)
                {
                    var customer = await _context.Customers.FindAsync(invoice.CustomerID);
                    if (customer != null)
                    {
                        var pointsToAdd = Math.Floor(invoice.TotalAmount.Value / 10000);
                        customer.Point = (customer.Point ?? 0) + pointsToAdd;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thanh toán thành công",
                    invoiceId = invoice.InvoiceID.ToString()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ✅ Xử lý thanh toán PayOS cho Product
        [HttpPost]
        public async Task<IActionResult> ProcessProductPayOsPayment([FromBody] PaymentRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.InvoiceId) || !Guid.TryParse(request.InvoiceId, out Guid invoiceGuid))
                {
                    return Json(new { success = false, message = "Invoice ID không hợp lệ" });
                }

                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceProducts)
                        .ThenInclude(ip => ip.Product)
                    .FirstOrDefaultAsync(i => i.InvoiceID == invoiceGuid && !i.IsDeleted);

                if (invoice == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                if (invoice.Status == "Đã thanh toán")
                {
                    return Json(new { success = false, message = "Hóa đơn đã được thanh toán" });
                }

                // ✅ Lấy thông tin khách hàng
                string buyerName = invoice.Customer?.FullName ?? "Khách vãng lai";
                string buyerEmail = invoice.Customer?.Email ?? "guest@cinestar.com";
                string buyerPhone = invoice.Customer?.Phone ?? "0000000000";

                // ✅ Tạo mô tả từ danh sách sản phẩm
                var productNames = string.Join(", ",
                    invoice.InvoiceProducts.Select(ip => ip.Product?.ProductName ?? "Sản phẩm")
                );
                var description = $"Thanh toán bắp nước - {productNames}";

                // ✅ Tạo payment link
                var paymentResult = await _payOsService.CreateTicketPaymentLink(
                    invoiceGuid,
                    invoice.TotalAmount.Value,
                    buyerName,
                    buyerEmail,
                    buyerPhone,
                    description,
                    isAdminSale: true
                );

                if (paymentResult == null)
                {
                    return Json(new { success = false, message = "Không thể tạo link thanh toán" });
                }

                return Json(new
                {
                    success = true,
                    message = "Tạo link thanh toán thành công",
                    paymentUrl = paymentResult.checkoutUrl,
                    orderCode = paymentResult.orderCode
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ✅ DTO cho payment request
        public class PaymentRequestDto
        {
            public string InvoiceId { get; set; }
        }

        ////realtime chọn ghế
        //// Render ra các ghế 
        //public async Task<IActionResult> GetSeatingLayout(string showTimeId, Guid currentCustomerId)
        //{
        //    var seats = await _movieService_Cus.GetSeatingLayoutAsync(showTimeId, currentCustomerId);
        //    return Json(seats);
        //}

        //// Xử lý click đặt ghế
        //[HttpPost]
        //public async Task<IActionResult> SelectSeats(string showTimeId, string seatId)
        //{
        //    var customerId = User.FindFirstValue("CustomerID");
        //    Console.WriteLine(" ------------ " + showTimeId + " " + seatId + " " + customerId);
        //    var success = await _movieService_Cus.TrySelectSeatAsync(showTimeId, seatId, Guid.Parse(customerId));
        //    return Json(new { success });
        //}

        //[HttpPost]
        //public async Task<IActionResult> DeselectSeat(string showTimeId, string seatId)
        //{
        //    var customerId = User.FindFirstValue("CustomerID");
        //    var success = await _movieService_Cus.DeselectSeatAsync(showTimeId, seatId, Guid.Parse(customerId));
        //    return Json(new { success });
        //}
    }
}
