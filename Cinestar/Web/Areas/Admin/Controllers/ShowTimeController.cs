using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShowTimeController : Controller
    {
        private readonly IShowTimeService _service;

        public ShowTimeController(IShowTimeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _service.GetAllAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(string id)
        {
            var st = await _service.GetByIdAsync(id);
            if (st == null) return NotFound();
            return View(st);
        }

        public IActionResult Create()
        {
            return View(new ShowTime());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowTime model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _service.CreateAsync(model);
            if (result)
                return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Thêm suất chiếu thất bại");
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var st = await _service.GetByIdAsync(id);
            if (st == null) return NotFound();
            return View(st);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ShowTime model)
        {
            if (id != model.ShowTimeID) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var result = await _service.UpdateAsync(model);
            if (result)
                return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Cập nhật thất bại");
            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var st = await _service.GetByIdAsync(id);
            if (st == null) return NotFound();
            return View(st);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _service.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            await _service.RestoreAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
