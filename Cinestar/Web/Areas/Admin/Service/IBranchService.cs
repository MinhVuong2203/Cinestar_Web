using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface IBranchService
    {
        public Task<List<CinemaBranch>> GetCinemaBranches();
        public Task CreateCinemaBranch(CinemaBranch branch);
        public Task DeteleBranch(string branchId);
    }
}
