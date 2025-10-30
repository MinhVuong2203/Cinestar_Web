using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BranchController : Controller
    {
        private readonly CineStarContext _context;
        public BranchController(CineStarContext context)
        {
            _context = context;
        }
        
       // GET: /Admin/Branch
        public async Task<IActionResult> Index()
        {
            var branches = await _context.CinemaBranches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.City)
                .ThenBy(b => b.BranchName)
                .ToListAsync();
            return View(branches);
        }

        // CREATE 


    }
}
