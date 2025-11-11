using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Service
{
    public class ProductService : IProductService
    {
        private readonly CineStarContext _context;

        public ProductService(CineStarContext context)
        {
            _context = context;
            Console.WriteLine("[ProductService] Initialized with DbContext ✅");
        }

        public async Task<List<Product>> GetProductsByTypeAsync(string productType)
        {
            try
            {
                Console.WriteLine($"[GetProductsByType] Searching for '{productType}'");

                var products = await _context.Products
                    .Where(p => p.ProductType == productType && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[GetProductsByType] ✅ Found {products.Count} products for '{productType}'");

                foreach (var product in products)
                {
                    Console.WriteLine($"  → {product.ProductName} - {product.Price} VND");
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetProductsByType] ❌ Error: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<Dictionary<string, List<Product>>> GetAllProductsGroupedByTypeAsync()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("[GetAllProductsGrouped] START");
            Console.WriteLine("========================================");

            try
            {
                // Lấy tất cả sản phẩm và group by ProductType
                var allProducts = await _context.Products
                    .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.ProductType))
                    .OrderBy(p => p.ProductType)
                    .ThenBy(p => p.ProductName)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[GetAllProductsGrouped] Total products loaded: {allProducts.Count}");

                var groupedProducts = allProducts
                    .GroupBy(p => p.ProductType)
                    .ToDictionary(
                        g => g.Key ?? "Khác",
                        g => g.ToList()
                    );

                Console.WriteLine($"[GetAllProductsGrouped] Categories: {groupedProducts.Count}");

                foreach (var category in groupedProducts)
                {
                    Console.WriteLine($"  ✓ {category.Key}: {category.Value.Count} products");
                }

                Console.WriteLine("========================================");
                Console.WriteLine("[GetAllProductsGrouped] END ✅");
                Console.WriteLine("========================================");

                return groupedProducts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllProductsGrouped] ❌ ERROR: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return new Dictionary<string, List<Product>>();
            }
        }
    }
}