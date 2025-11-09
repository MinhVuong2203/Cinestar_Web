using Net.payOS.Types;
using Web.Models;

namespace Web.Service
{
    public interface IPayOsService
    {
        /// <summary>
        /// Tạo link thanh toán mới
        /// </summary>
        /// <param name="paymentData">Thông tin yêu cầu thanh toán</param>
        /// <returns>Kết quả tạo link thanh toán</returns>
        Task<CreatePaymentResult?> CreatePaymentLink(PaymentData paymentData);

        /// <summary>
        /// Lấy thông tin chi tiết của một payment link
        /// </summary>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <returns>Thông tin payment link</returns>
        Task<PaymentLinkInformation?> GetPaymentLinkInformation(long orderCode);

        /// <summary>
        /// Hủy payment link
        /// </summary>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <param name="reason">Lý do hủy (optional)</param>
        /// <returns>Thông tin payment link sau khi hủy</returns>
        Task<PaymentLinkInformation?> CancelPaymentLink(long orderCode, string? reason = null);

        /// <summary>
        /// Xác thực webhook từ PayOS
        /// </summary>
        /// <param name="webhookType">Loại webhook</param>
        /// <returns>Dữ liệu webhook đã được xác thực</returns>
        WebhookData? VerifyWebhookData(WebhookType webhookType);

        /// <summary>
        /// Tạo mã đơn hàng duy nhất
        /// </summary>
        /// <returns>Mã đơn hàng (timestamp)</returns>
        long GenerateOrderCode();

        /// <summary>
        /// Kiểm tra trạng thái thanh toán
        /// </summary>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <returns>True nếu đã thanh toán thành công</returns>
        Task<bool> IsPaymentSuccess(long orderCode);

        /// <summary>
        /// Lấy URL return về sau khi thanh toán
        /// </summary>
        /// <param name="success">True nếu thanh toán thành công, False nếu bị hủy</param>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <returns>URL return</returns>
        string GetReturnUrl(bool success, long orderCode);

        /// <summary>
        /// Tạo link thanh toán cho đơn đặt vé phim
        /// </summary>
        /// <param name="invoiceId">ID hóa đơn</param>
        /// <param name="amount">Số tiền</param>
        /// <param name="buyerName">Tên khách hàng</param>
        /// <param name="buyerEmail">Email khách hàng</param>
        /// <param name="buyerPhone">Số điện thoại khách hàng</param>
        /// <param name="description">Mô tả</param>
        /// <returns>Kết quả tạo link thanh toán</returns>
        Task<CreatePaymentResult?> CreateTicketPaymentLink(
        Guid invoiceId,
        decimal amount,
        string buyerName,
        string buyerEmail,
        string buyerPhone,
        string description);

        Task<CreatePaymentResult?> CreateTicketPaymentLink(
        Guid invoiceId,
        decimal amount,
        string buyerName,
        string buyerEmail,
        string buyerPhone,
        string description,
        bool isAdminSale = false);
    }
}
