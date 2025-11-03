using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;
using Web.Models.Configuration;
using Web.Helpers;

namespace Web.Service
{
    public class PayOsService : IPayOsService
    {
        private readonly PayOS _payOS;
        private readonly PayOsSettings _settings;
        private readonly IConfiguration _configuration;
    private readonly ILogger<PayOsService> _logger;

     public PayOsService(
         IOptions<PayOsSettings> settings,
       IConfiguration configuration,
 ILogger<PayOsService> logger)
     {
 _settings = settings.Value;
    _configuration = configuration;
 _logger = logger;

  // Initialize PayOS client
  _payOS = new PayOS(
     _settings.ClientId,
_settings.ApiKey,
         _settings.ChecksumKey
);
        }

        public async Task<CreatePaymentResult?> CreatePaymentLink(PaymentData paymentData)
        {
  try
  {
   // G?i API t?o payment link
    CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

  _logger.LogInformation("Payment link created successfully. OrderCode: {OrderCode}, PaymentLinkId: {PaymentLinkId}",
          paymentData.orderCode, createPayment.paymentLinkId);

             return createPayment;
     }
 catch (Exception ex)
        {
         _logger.LogError(ex, "Error creating payment link for OrderCode: {OrderCode}", paymentData.orderCode);
       throw;
   }
  }

   public async Task<PaymentLinkInformation?> GetPaymentLinkInformation(long orderCode)
  {
            try
  {
    PaymentLinkInformation paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

       _logger.LogInformation("Retrieved payment info for OrderCode: {OrderCode}, Status: {Status}",
      orderCode, paymentInfo.status);

  return paymentInfo;
  }
   catch (Exception ex)
  {
        _logger.LogError(ex, "Error getting payment link information for OrderCode: {OrderCode}", orderCode);
 return null;
 }
    }

        public async Task<PaymentLinkInformation?> CancelPaymentLink(long orderCode, string? reason = null)
 {
        try
  {
         PaymentLinkInformation cancelledPayment = await _payOS.cancelPaymentLink(orderCode, reason);

     _logger.LogInformation("Payment link cancelled for OrderCode: {OrderCode}, Reason: {Reason}",
      orderCode, reason ?? "No reason provided");

     return cancelledPayment;
    }
    catch (Exception ex)
            {
 _logger.LogError(ex, "Error cancelling payment link for OrderCode: {OrderCode}", orderCode);
     throw;
   }
   }

    public WebhookData? VerifyWebhookData(WebhookType webhookType)
 {
try
        {
  WebhookData webhookData = _payOS.verifyPaymentWebhookData(webhookType);

       _logger.LogInformation("Webhook verified for OrderCode: {OrderCode}", webhookData.orderCode);

   return webhookData;
  }
 catch (Exception ex)
{
     _logger.LogError(ex, "Error verifying webhook data");
       return null;
  }
 }

        public long GenerateOrderCode()
  {
  // S? d?ng timestamp ?? t?o order code duy nh?t
 return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public async Task<bool> IsPaymentSuccess(long orderCode)
  {
  try
  {
    var paymentInfo = await GetPaymentLinkInformation(orderCode);
        return paymentInfo?.status?.ToUpper() == "PAID";
  }
       catch
  {
  return false;
            }
      }

 public string GetReturnUrl(bool success, long orderCode)
        {
      var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7000";

 if (success)
    {
        return $"{baseUrl}/Payment/PaymentSuccess?orderCode={orderCode}";
     }
            else
  {
       return $"{baseUrl}/Payment/PaymentCancel?orderCode={orderCode}";
   }
 }

        public async Task<CreatePaymentResult?> CreateTicketPaymentLink(
      Guid invoiceId,
       decimal amount,
 string buyerName,
     string buyerEmail,
       string buyerPhone,
      string description)
 {
  try
       {
              var orderCode = GenerateOrderCode();
       var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7000";

       // Chu?n hóa d? li?u ??u vào
var validatedAmount = PayOsHelper.ValidateAmount(amount);
      var normalizedDescription = PayOsHelper.NormalizeDescription(description);
 var normalizedItemName = PayOsHelper.NormalizeItemName("Ve xem phim");
  var validatedEmail = PayOsHelper.ValidateEmail(buyerEmail);
     var validatedPhone = PayOsHelper.ValidatePhoneNumber(buyerPhone);

  var items = new List<ItemData>
  {
   new ItemData(normalizedItemName, 1, validatedAmount)
 };

 var paymentData = new PaymentData(
         orderCode: orderCode,
   amount: validatedAmount,
    description: normalizedDescription,
 items: items,
 cancelUrl: $"{baseUrl}/Payment/PaymentCancel?invoiceId={invoiceId}",
      returnUrl: $"{baseUrl}/Payment/PaymentSuccess?invoiceId={invoiceId}",
   buyerName: buyerName,
    buyerEmail: validatedEmail,
      buyerPhone: validatedPhone,
     buyerAddress: "Viet Nam",
   expiredAt: (int)DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
         );

     return await CreatePaymentLink(paymentData);
       }
       catch (Exception ex)
     {
         _logger.LogError(ex, "Error creating ticket payment link for InvoiceId: {InvoiceId}", invoiceId);
throw;
     }
 }
    }
}
