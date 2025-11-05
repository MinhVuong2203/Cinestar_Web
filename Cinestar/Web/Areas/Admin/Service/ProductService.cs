using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
        public class ProductService : IProductService
        {
            private readonly CineStarContext _context;
            private readonly IWebHostEnvironment _webHostEnvironment;

            public ProductService(CineStarContext context, IWebHostEnvironment webHostEnvironment)
            {
                _context = context;
                _webHostEnvironment = webHostEnvironment;
            }

            public async Task<IEnumerable<Product>> GetAllProductsAsync(bool includeDeleted = false)
            {
                var query = _context.Products.AsQueryable();

                if (!includeDeleted)
                {
                    query = query.Where(p => !p.IsDeleted);
                }

                return await query.OrderBy(p => p.ProductName).ToListAsync();
            }

            public async Task<IEnumerable<Product>> SearchProductsAsync(string searchString, string productType, bool showDeleted = false)
            {
                var query = _context.Products.AsQueryable();

                if (!showDeleted)
                {
                    query = query.Where(p => !p.IsDeleted);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(p => p.ProductName.Contains(searchString) ||
                                            p.ProductID.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(productType))
                {
                    query = query.Where(p => p.ProductType == productType);
                }

                return await query.OrderBy(p => p.ProductName).ToListAsync();
            }

            public async Task<Product?> GetProductByIdAsync(string productId)
            {
                return await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductID == productId);
            }

            public async Task<List<string>> GetProductTypesAsync()
            {
                return await _context.Products
                    .Where(p => !string.IsNullOrEmpty(p.ProductType))
                    .Select(p => p.ProductType!)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
            }

            public async Task<bool> CreateProductAsync(Product product, IFormFile? imageFile)
            {
                try
                {
                    // Xử lý upload ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        product.ImageUrl = await SaveImageAsync(imageFile);
                    }
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public async Task<bool> UpdateProductAsync(Product product, IFormFile? imageFile)
            {
                try
                {
                    var existingProduct = await GetProductByIdAsync(product.ProductID);
                    if (existingProduct == null) return false;

                    // Xử lý upload ảnh mới
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            DeleteImage(existingProduct.ImageUrl);
                        }

                        product.ImageUrl = await SaveImageAsync(imageFile);
                    }
                    else
                    {
                        // Giữ nguyên ảnh cũ
                        product.ImageUrl = existingProduct.ImageUrl;
                    }

                    _context.Entry(existingProduct).CurrentValues.SetValues(product);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public async Task<bool> SoftDeleteProductAsync(string productId)
            {
                try
                {
                    var product = await GetProductByIdAsync(productId);
                    if (product == null) return false;

                    product.IsDeleted = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public async Task<bool> RestoreProductAsync(string productId)
            {
                try
                {
                    var product = await GetProductByIdAsync(productId);
                    if (product == null) return false;

                    product.IsDeleted = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public async Task<bool> ProductExistsAsync(string productId)
            {
                return await _context.Products.AnyAsync(p => p.ProductID == productId);
            }

            public async Task<bool> IsProductInUseAsync(string productId)
            {
                var hasInvoices = await _context.InvoiceProducts
                    .AnyAsync(ip => ip.ProductID == productId);

                var hasMovieProducts = await _context.MovieProducts
                    .AnyAsync(mp => mp.ProductID == productId);

                return hasInvoices || hasMovieProducts;
            }

            // Private helper methods
            private async Task<string> SaveImageAsync(IFormFile imageFile)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image", "product");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return "/image/product/" + uniqueFileName;
            }

            private void DeleteImage(string imageUrl)
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    
}
