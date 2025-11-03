using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web.Areas.Admin.Service;
using Web.Data;
using Web.Models;


namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BranchController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBranchService _branchService;
        public BranchController(IBranchService branchService, IWebHostEnvironment webHostEnvironment)
        {
            _branchService = branchService;
            _webHostEnvironment = webHostEnvironment;
        }
        
       // GET: /Admin/Branch
        public async Task<IActionResult> Index()
        {
            var branches = await _branchService.GetCinemaBranches();
            return View(branches);
        }

        // GET: /Admin/Branch/Create 
        public IActionResult Create()
        {
            return View();
        }
        // POST: /Admin/Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CinemaBranch branch, IFormFile ImageFile)
        {
            if (branch.OpenHour != null && branch.CloseHour != null)
            {
                TimeSpan duration = branch.CloseHour.ToTimeSpan() - branch.OpenHour.ToTimeSpan();

                if (duration.TotalHours <= 0)
                {
                    ModelState.AddModelError(nameof(branch.CloseHour), "Giờ đóng cửa phải lớn hơn giờ mở cửa");
                }
                else if (duration.TotalHours < 1)
                {
                    ModelState.AddModelError(nameof(branch.CloseHour), "Rạp phải mở cửa ít nhất 1 tiếng");
                }
            }
            // ✅ Nếu có lỗi, render lại View (giữ nguyên dữ liệu nhập)
            if (!ModelState.IsValid)
            {
                return View(branch);
            }
            ModelState.Clear();
            // --- Xử lý upload file ---
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "image", "branches");
                // Tạo thư mục nếu chưa có
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);
                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                // Cập nhật URL
                branch.ImageUrl = $"/image/branches/{fileName}";
            }

            await _branchService.CreateCinemaBranch(branch);
            TempData["Success"] = "Thêm chi nhánh thành công!";
            return RedirectToAction(nameof(Index)); 
        }


        // GET: /Admin/Branch/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var branch = await _branchService.GetCinemaBranch(id);

            if (branch == null)
            {
                return NotFound();
            }
            return View(branch);
        }

        // POST: /Admin/Branch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CinemaBranch branch, IFormFile ImageFile)
        {
            if (id != branch.BranchID)
            {
                return NotFound();
            }
            if (branch.OpenHour != null && branch.CloseHour != null)
            {
                TimeSpan duration = branch.CloseHour.ToTimeSpan() - branch.OpenHour.ToTimeSpan();

                if (duration.TotalHours <= 0)
                {
                    ModelState.AddModelError(nameof(branch.CloseHour), "Giờ đóng cửa phải lớn hơn giờ mở cửa");
                }
                else if (duration.TotalHours < 1)
                {
                    ModelState.AddModelError(nameof(branch.CloseHour), "Rạp phải mở cửa ít nhất 1 tiếng");
                }
            }
            // ✅ Nếu có lỗi, render lại View (giữ nguyên dữ liệu nhập)
            if (!ModelState.IsValid)
            {
                return View(branch);
            }
            else  
            {
                ModelState.Clear();
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Tạo tên file unique
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "image", "branches");

                        // Tạo thư mục nếu chưa có
                        Directory.CreateDirectory(uploadPath);

                        var filePath = Path.Combine(uploadPath, fileName);

                        // Lưu file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Cập nhật URL
                        branch.ImageUrl = $"/image/branches/{fileName}";
                    }
                    // ✅ Nếu không upload ảnh mới, giữ nguyên ImageUrl cũ
                    else
                    {
                        var existingBranch = await _branchService.GetCinemaBranch(id);

                        if (existingBranch != null)
                        {
                            branch.ImageUrl = existingBranch.ImageUrl;
                        }
                    }
                    await _branchService.EditBranch(branch);
                    TempData["Success"] = "Cập nhật chi nhánh thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: /Admin/Branch/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string BranchId)
        {
            if (ModelState.IsValid)
            {
                await _branchService.DeteleBranch(BranchId);
                TempData["Success"] = "Xóa chi nhánh thành công!";
            }
            return RedirectToAction(nameof(Index));   
        }



    }
}
