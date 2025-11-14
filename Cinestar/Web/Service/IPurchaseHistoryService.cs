using Web.Models.DTOs;

namespace Web.Service
{
    public interface IPurchaseHistoryService
    {
        //lấy thông tin vé đã mua của khách hàng
        List<PurchasedTicketDto> GetPurchasedTicketsByCustomerId(Guid customerId);
    }
}
