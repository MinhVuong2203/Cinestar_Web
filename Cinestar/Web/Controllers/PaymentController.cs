using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Web.Service;
using Net.payOS.Types;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPayOsService _payOsService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPayOsService payOsService, ILogger<PaymentController> logger)
        {
            _payOsService = payOsService;
            _logger = logger;
        }

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

        [HttpPost]
        public async Task<IActionResult> CreatePayOsPayment([FromBody] PaymentRequest request)
        {
            try
            {
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
                    return Json(new { success = true, checkoutUrl = result.checkoutUrl, qrCode = result.qrCode });
                }

                return Json(new { success = false, message = "Không thể tạo link thanh toán" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment");
                return Json(new { success = false, message = ex.Message });
            }
        }

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

        [HttpGet]
        public IActionResult PaymentCancel(long orderCode)
        {
            ViewBag.OrderCode = orderCode;
            return View();
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

                    // Xử lý logic cập nhật database ở đây
                    // Ví dụ: Cập nhật trạng thái Invoice, Payment

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
    }
}
