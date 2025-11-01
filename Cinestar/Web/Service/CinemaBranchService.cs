using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Service
{
    public class CinemaBranchService : ICinemaBranchService
    {
        private readonly CineStarContext _context;
        public CinemaBranchService(CineStarContext context) { 
            this._context = context;
        }

        public List<CinemaBranch> GetBranches()
        {
            try {
                return _context.CinemaBranches.Where(b => !b.IsDeleted).AsNoTracking().ToList();
            }
            catch { 
                return new List<CinemaBranch>();
            }
        }

    }
}
