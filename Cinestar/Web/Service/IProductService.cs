using Web.Models;

namespace Web.Service
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsByTypeAsync(string productType);
        Task<Dictionary<string, List<Product>>> GetAllProductsGroupedByTypeAsync();
    }
}