using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.payOS.Types;
using Newtonsoft.Json;
using Web.Data;
using Web.Models;
using Web.Service;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPayOsService _payOsService;
        private readonly ILogger<PaymentController> _logger;
        private readonly CineStarContext _context;


        public PaymentController(IPayOsService payOsService, ILogger<PaymentController> logger, CineStarContext context)
        {
            _payOsService = payOsService;
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var customerIdStr = User.FindFirst("CustomerID")?.Value;
                if (Guid.TryParse(customerIdStr, out Guid customerId))
                {
                    var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId && !c.IsDeleted);
                    if (customer != null)
                    {
                        ViewData["LoggedCustomer"] = customer;
                    }
                }
            }
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

        //[HttpPost]
        //public async Task<IActionResult> CreatePayOsPayment([FromBody] PaymentRequest request)
        //{
        //    try
        //    {
        //        var items = new List<ItemData>
        //        {
        //            new ItemData(request.Description, 1, request.Amount)
        //        };

        //        var paymentData = new PaymentData(
        //            orderCode: request.OrderCode,
        //            amount: request.Amount,
        //            description: request.Description,
        //            items: items,
        //            cancelUrl: request.CancelUrl,
        //            returnUrl: request.ReturnUrl,
        //            buyerName: request.BuyerName,
        //            buyerEmail: request.BuyerEmail,
        //            buyerPhone: request.BuyerPhone,
        //            buyerAddress: request.BuyerAddress,
        //            expiredAt: request.ExpiredAt
        //        );

        //        var result = await _payOsService.CreatePaymentLink(paymentData);

        //        if (result != null)
        //        {
        //            return Json(new { success = true, checkoutUrl = result.checkoutUrl, qrCode = result.qrCode });
        //        }

        //        return Json(new { success = false, message = "Không thể tạo link thanh toán" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating PayOS payment");
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> CreatePayOsPayment([FromBody] PaymentRequest request)
        {
            try
            {
                // ✅ LƯU INVOICE TRƯỚC KHI TẠO PAYMENT LINK
                var invoice = await CreateInvoiceFromBooking(request.BookingData);

                if (invoice == null)
                {
                    return Json(new { success = false, message = "Không thể tạo hóa đơn" });
                }

                var items = new List<ItemData>
                {
                    new ItemData(request.Description, 1, request.Amount)
                };

                var paymentData = new PaymentData(
                    orderCode: request.OrderCode,
                    amount: request.Amount,
                    description: request.Description,
                    items: items,
                    cancelUrl: request.CancelUrl,
                    returnUrl: request.ReturnUrl,
                    buyerName: request.BuyerName,
                    buyerEmail: request.BuyerEmail,
                    buyerPhone: request.BuyerPhone,
                    buyerAddress: request.BuyerAddress,
                    expiredAt: request.ExpiredAt
                );

                var result = await _payOsService.CreatePaymentLink(paymentData);

                if (result != null)
                {
                    // ✅ Lưu mapping OrderCode -> InvoiceID vào Session
                    HttpContext.Session.SetString($"OrderCode_{request.OrderCode}", invoice.InvoiceID.ToString());

                    _logger.LogInformation("Created invoice {InvoiceID} for OrderCode {OrderCode}",
                        invoice.InvoiceID, request.OrderCode);

                    return Json(new { success = true, checkoutUrl = result.checkoutUrl, qrCode = result.qrCode });
                }

                // ✅ Nếu tạo payment link thất bại, hủy invoice
                await CancelInvoice(invoice.InvoiceID);

                return Json(new { success = false, message = "Không thể tạo link thanh toán" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ✅ HÀM TẠO INVOICE TỪ BOOKING DATA
        private async Task<Invoice?> CreateInvoiceFromBooking(BookingData? bookingData)
        {
            if (bookingData == null)
            {
                _logger.LogWarning("❌ BookingData is null");
                return null;
            }

            _logger.LogInformation("=== CREATE INVOICE FROM BOOKING ===");
            _logger.LogInformation("ShowTimeId: {ShowTimeId}", bookingData.ShowTimeId);
            _logger.LogInformation("CustomerId: {CustomerId}", bookingData.CustomerId);
            _logger.LogInformation("Seats Count: {Count}", bookingData.Seats?.Count ?? 0);
            _logger.LogInformation("Products Count: {Count}", bookingData.Products?.Count ?? 0);

            if (string.IsNullOrEmpty(bookingData.ShowTimeId))
            {
                _logger.LogWarning("❌ ShowTimeId is empty or null");
                return null;
            }

            try
            {
                // ✅ Validate showtime
                var showTime = await _context.ShowTimes
                    .Include(st => st.Movie)
                    .Include(st => st.Room)
                        .ThenInclude(r => r.Branch)
                    .FirstOrDefaultAsync(st => st.ShowTimeID == bookingData.ShowTimeId && !st.IsDeleted);

                if (showTime == null)
                {
                    _logger.LogWarning("❌ ShowTime not found: {ShowTimeId}", bookingData.ShowTimeId);
                    return null;
                }

                _logger.LogInformation("✅ ShowTime found: {ShowTimeId}, Movie: {MovieTitle}",
                    showTime.ShowTimeID, showTime.Movie?.Title);

                // ✅ Xử lý CustomerId
                Guid? customerGuid = null;
                if (bookingData.CustomerId != Guid.Empty)
                {
                    customerGuid = bookingData.CustomerId;
                    _logger.LogInformation("✅ Customer ID: {CustomerId}", customerGuid);
                }
                else
                {
                    _logger.LogInformation("ℹ️ Guest booking (no customer ID)");
                }

                // ✅ Tính tổng tiền
                decimal seatsTotal = bookingData.Seats?.Sum(s => s.Price) ?? 0;
                decimal productsTotal = bookingData.Products?.Sum(p => p.Price * p.Quantity) ?? 0;
                decimal totalAmount = seatsTotal + productsTotal;

                _logger.LogInformation("💰 Seats Total: {SeatsTotal}, Products Total: {ProductsTotal}, Total: {Total}",
                    seatsTotal, productsTotal, totalAmount);

                // ✅ Tạo Invoice
                var invoice = new Invoice
                {
                    InvoiceID = Guid.NewGuid(),
                    EmployeeID = null,
                    CustomerID = customerGuid,
                    BranchID = showTime.Room?.BranchID,
                    IssueDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    Discount = 0,
                    Status = "Chờ thanh toán",
                    IsDeleted = false
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Invoice created: {InvoiceID}", invoice.InvoiceID);

                // ✅ Tạo InvoiceTicket
                if (bookingData.Seats != null && bookingData.Seats.Count > 0)
                {
                    _logger.LogInformation("📝 Processing {Count} seats", bookingData.Seats.Count);

                    foreach (var seatData in bookingData.Seats)
                    {
                        _logger.LogInformation("  Processing seat: {SeatId}, TicketId: {TicketId}",
                            seatData.SeatId, seatData.TicketId);

                        if (!Guid.TryParse(seatData.TicketId, out Guid ticketGuid))
                        {
                            _logger.LogWarning("❌ Invalid TicketId format: {TicketId}", seatData.TicketId);
                            continue;
                        }

                        var ticket = await _context.Tickets
                            .FirstOrDefaultAsync(t =>
                                t.TicketID == ticketGuid &&
                                t.ShowTimeID == bookingData.ShowTimeId &&
                                !t.IsDeleted);

                        if (ticket == null)
                        {
                            _logger.LogWarning("❌ Ticket not found: {TicketId}", ticketGuid);
                            await _context.Database.BeginTransactionAsync();
                            _context.Invoices.Remove(invoice);
                            await _context.SaveChangesAsync();
                            await _context.Database.CommitTransactionAsync();
                            return null;
                        }

                        // ✅ SỬA LẠI: Chấp nhận "Trống" hoặc "Đang được chọn" bởi customer này
                        bool isAvailable = ticket.Status == "Trống" ||
                                          (ticket.Status == "Đang được chọn" && ticket.LockedBy == customerGuid);


                        if (!isAvailable)
                        {
                            _logger.LogWarning("❌ Ticket {TicketId} not available (Status: {Status})",
                                ticketGuid, ticket.Status);
                            await _context.Database.BeginTransactionAsync();
                            _context.Invoices.Remove(invoice);
                            await _context.SaveChangesAsync();
                            await _context.Database.CommitTransactionAsync();
                            return null;
                        }

                        // Lock ticket
                        ticket.Status = "Đã đặt";
                        ticket.LockedBy = customerGuid;
                        ticket.LockedAt = DateTime.Now;

                        var invoiceTicket = new InvoiceTicket
                        {
                            InvoiceID = invoice.InvoiceID,
                            TicketID = ticketGuid,
                            Quantity = 1,
                            UnitPrice = seatData.Price
                        };

                        _context.InvoiceTickets.Add(invoiceTicket);
                        _logger.LogInformation("  ✅ Ticket {TicketId} locked", ticketGuid);
                    }
                }

                // ✅ Tạo InvoiceProduct
                if (bookingData.Products != null && bookingData.Products.Count > 0)
                {
                    _logger.LogInformation("🍿 Processing {Count} products", bookingData.Products.Count);

                    foreach (var product in bookingData.Products)
                    {
                        if (!Guid.TryParse(product.ProductId, out Guid productGuid))
                        {
                            _logger.LogWarning("❌ Invalid ProductId format: {ProductId}", product.ProductId);
                            continue;
                        }

                        var invoiceProduct = new InvoiceProduct
                        {
                            InvoiceID = invoice.InvoiceID,
                            ProductID = product.ProductId,
                            Quantity = product.Quantity,
                            UnitPrice = product.Price
                        };

                        _context.InvoiceProducts.Add(invoiceProduct);
                        _logger.LogInformation("  ✅ Product {ProductId} added (Qty: {Qty})",
                            product.ProductId, product.Quantity);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅✅✅ Invoice {InvoiceID} created successfully!", invoice.InvoiceID);

                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating invoice from booking data");
                return null;
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> PaymentSuccess(long orderCode)
        //{
        //    try
        //    {
        //        var paymentInfo = await _payOsService.GetPaymentLinkInformation(orderCode);

        //        if (paymentInfo == null)
        //        {
        //            TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán";
        //            return RedirectToAction("Index");
        //        }

        //        ViewBag.PaymentInfo = paymentInfo;
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting payment success info");
        //        TempData["ErrorMessage"] = "Có lỗi xảy ra khi xác nhận thanh toán";
        //        return RedirectToAction("Index");
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(long orderCode)
        {
            try
            {
                var paymentInfo = await _payOsService.GetPaymentLinkInformation(orderCode);

                if (paymentInfo == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán";
                    return RedirectToAction("Index");
                }

                // ✅ Cập nhật trạng thái Invoice và Payment nếu thanh toán thành công
                if (paymentInfo.status?.ToUpper() == "PAID")
                {
                    // Lấy InvoiceID từ session
                    var invoiceIdStr = HttpContext.Session.GetString($"OrderCode_{orderCode}");

                    if (!string.IsNullOrEmpty(invoiceIdStr) && Guid.TryParse(invoiceIdStr, out Guid invoiceId))
                    {
                        var invoice = await _context.Invoices
                            .Include(i => i.Customer)
                            .Include(i => i.InvoiceTickets)
                                .ThenInclude(it => it.Ticket)
                            .FirstOrDefaultAsync(i => i.InvoiceID == invoiceId && !i.IsDeleted);

                        if (invoice != null)
                        {
                            // ✅ Cập nhật invoice status
                            invoice.Status = "Đã thanh toán";

                            // ✅ Cập nhật ticket status
                            foreach (var invoiceTicket in invoice.InvoiceTickets)
                            {
                                invoiceTicket.Ticket.Status = "Đã thanh toán";
                                invoiceTicket.Ticket.LockedBy = null;
                                invoiceTicket.Ticket.LockedAt = null;
                            }

                            // ✅ Tạo payment record
                            var payment = new Payment
                            {
                                PaymentID = Guid.NewGuid(),
                                InvoiceID = invoice.InvoiceID,
                                Method = "PayOS",
                                Amount = paymentInfo?.amount ?? 0,
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

                            _logger.LogInformation("✅ Payment completed for Invoice {InvoiceID}, OrderCode {OrderCode}",
                                invoice.InvoiceID, orderCode);

                            // ✅ Xóa session
                            HttpContext.Session.Remove($"OrderCode_{orderCode}");
                        }
                        else
                        {
                            _logger.LogWarning("Invoice not found for OrderCode {OrderCode}", orderCode);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No invoice mapping found for OrderCode {OrderCode}", orderCode);
                    }
                }

                ViewBag.PaymentInfo = paymentInfo;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment success info");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xác nhận thanh toán";
                return RedirectToAction("Index");
            }
        }

        //[HttpGet]
        //public IActionResult PaymentCancel(long orderCode)
        //{
        //    ViewBag.OrderCode = orderCode;
        //    return View();
        //}
        [HttpGet]
        public async Task<IActionResult> PaymentCancel(long orderCode)
        {
            // ✅ Hủy invoice khi cancel payment
            var invoiceIdStr = HttpContext.Session.GetString($"OrderCode_{orderCode}");

            if (!string.IsNullOrEmpty(invoiceIdStr) && Guid.TryParse(invoiceIdStr, out Guid invoiceId))
            {
                await CancelInvoice(invoiceId);
            }

            HttpContext.Session.Remove($"OrderCode_{orderCode}");

            ViewBag.OrderCode = orderCode;
            return View();
        }

        // ✅ HÀM HỦY INVOICE
        private async Task CancelInvoice(Guid invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.InvoiceTickets)
                        .ThenInclude(it => it.Ticket)
                    .FirstOrDefaultAsync(i => i.InvoiceID == invoiceId && !i.IsDeleted);

                if (invoice != null)
                {
                    invoice.Status = "Đã hủy";

                    // Unlock tickets
                    foreach (var invoiceTicket in invoice.InvoiceTickets)
                    {
                        invoiceTicket.Ticket.Status = "Trống";
                        invoiceTicket.Ticket.LockedBy = null;
                        invoiceTicket.Ticket.LockedAt = null;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ Cancelled invoice {InvoiceID}", invoiceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling invoice {InvoiceID}", invoiceId);
            }
        }

        [HttpPost]
        public IActionResult PayOsWebhook([FromBody] WebhookType webhookType)
        {
            try
            {
                var webhookData = _payOsService.VerifyWebhookData(webhookType);

                if (webhookData != null)
                {
                    _logger.LogInformation("Webhook received for OrderCode: {OrderCode}, Status: {Code}",
                        webhookData.orderCode, webhookData.code);

                    // ✅ Xử lý cập nhật Invoice và Payment dựa trên webhook
                    // Logic tương tự PaymentSuccess

                    return Ok();
                }

                return BadRequest("Invalid webhook data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(long orderCode)
        {
            try
            {
                var isSuccess = await _payOsService.IsPaymentSuccess(orderCode);
                return Json(new { success = isSuccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelPayment(long orderCode, string? reason)
        {
            try
            {
                var result = await _payOsService.CancelPaymentLink(orderCode, reason);

                if (result != null)
                {
                    return Json(new { success = true, message = "Đã hủy thanh toán thành công" });
                }

                return Json(new { success = false, message = "Không thể hủy thanh toán" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // Payment Request DTO
    public class PaymentRequest
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public string BuyerAddress { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public int? ExpiredAt { get; set; }
        public BookingData? BookingData { get; set; } // ✅ Thêm
    }

    // ✅ Booking Data DTO
    public class BookingData
    {
        public string ShowTimeId { get; set; } = string.Empty;
        public List<SeatData> Seats { get; set; } = new();
        public List<ProductData> Products { get; set; } = new();
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
    }

    public class SeatData
    {
        public string SeatId { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string SeatName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class ProductData
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
