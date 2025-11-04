using Web.Models;

namespace Web.Service
{
    public interface ICinemaBranchService
    {
        public List<CinemaBranch> GetBranches();
        //lấy danh sách các thành phố có rạp
        public List<string> GetListCityBranches();
    }
}
