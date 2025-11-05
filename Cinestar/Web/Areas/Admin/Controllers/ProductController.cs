using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;
using Web.Models;
using Web.Service;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: Admin/Product
        public async Task<IActionResult> Index(string searchString, string productType, bool showDeleted = false)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["ProductType"] = productType;
            ViewData["ShowDeleted"] = showDeleted;

            // Lấy danh sách loại sản phẩm để hiển thị dropdown
            ViewBag.ProductTypes = await _productService.GetProductTypesAsync();

            var products = await _productService.SearchProductsAsync(searchString, productType, showDeleted);
            return View(products);
        }

        // GET: Admin/Product/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,ProductType,Price,ImageUrl")] Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var result = await _productService.CreateProductAsync(product, imageFile);

                if (result)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm sản phẩm!";
                }
            }

            return View(product);
        }

        // GET: Admin/Product/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProductID,ProductName,ProductType,Price,ImageUrl,IsDeleted")] Product product, IFormFile? imageFile)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _productService.UpdateProductAsync(product, imageFile);

                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật sản phẩm!";
                }
            }

            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            // Kiểm tra xem sản phẩm có đang được sử dụng không
            var isInUse = await _productService.IsProductInUseAsync(id);

            if (isInUse)
            {
                TempData["ErrorMessage"] = "Không thể ngừng bán sản phẩm này vì đang được sử dụng trong hóa đơn hoặc combo phim!";
                return RedirectToAction(nameof(Index));
            }

            var result = await _productService.SoftDeleteProductAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Ngừng bán sản phẩm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi ngừng bán sản phẩm!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Product/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _productService.RestoreProductAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Khôi phục sản phẩm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi khôi phục sản phẩm!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}