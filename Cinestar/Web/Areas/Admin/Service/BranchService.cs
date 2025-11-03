using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class BranchService : IBranchService
    {
        private readonly CineStarContext _context;
        public BranchService(CineStarContext context)
        {
            this._context = context;
        }

        public async Task<CinemaBranch> GetCinemaBranch(string Id)
        {
            var branch = await _context.CinemaBranches
                .FirstOrDefaultAsync(b => b.BranchID == Id && !b.IsDeleted);
            return branch;
        }
        

    
        // read
        public async Task<List<CinemaBranch>> GetCinemaBranches()
        {
            List<CinemaBranch> branch = await _context.CinemaBranches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.City)
                .ThenBy(b => b.BranchName)
                .ToListAsync();
            return branch;  
        }

        // create 
        public async Task CreateCinemaBranch(CinemaBranch branch) {
            branch.IsDeleted = false;
            _context.CinemaBranches.Add(branch);
            await _context.SaveChangesAsync();
        }

        public async Task EditBranch(CinemaBranch branch)
        {
            try
            {
                _context.Update(branch);
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        // delete
        public async Task DeteleBranch(string branchId)
        {
            CinemaBranch branch = await _context.CinemaBranches.FindAsync(branchId);
            if (branch != null)
            {
                branch.IsDeleted = true; // Soft delete
                _context.SaveChanges();
            }
           
        }


    }
}
