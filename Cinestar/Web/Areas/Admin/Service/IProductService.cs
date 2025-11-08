using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(bool includeDeleted = false);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchString, string productType, bool showDeleted = false);
        Task<Product?> GetProductByIdAsync(string productId);
        Task<List<string>> GetProductTypesAsync();
        Task<bool> CreateProductAsync(Product product, IFormFile? imageFile);
        Task<bool> UpdateProductAsync(Product product, IFormFile? imageFile);
        Task<bool> SoftDeleteProductAsync(string productId);
        Task<bool> RestoreProductAsync(string productId);
        Task<bool> ProductExistsAsync(string productId);
        Task<bool> IsProductInUseAsync(string productId);

        //lấy danh sách loại sản phẩm
        List<Product> GetAllProduct();
    }
}
