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
        private readonly IBranchService _branchService;
        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
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
        public async Task<IActionResult> Create(CinemaBranch branch)
        {
    
            if (ModelState.IsValid)
            {
                await _branchService.CreateCinemaBranch(branch);
                TempData["Success"] = "Thêm chi nhánh thành công!";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "Đã xảy ra lỗi!";
            return View(branch);
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
