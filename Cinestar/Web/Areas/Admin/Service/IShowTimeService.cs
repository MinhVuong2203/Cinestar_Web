using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface IShowTimeService
    {
        Task<List<ShowTime>> GetAllAsync();
        Task<ShowTime?> GetByIdAsync(string id);
        Task<bool> CreateAsync(ShowTime showTime);
        Task<bool> UpdateAsync(ShowTime showTime);
        Task<bool> SoftDeleteAsync(string id);
        Task<bool> RestoreAsync(string id);
    }
}
