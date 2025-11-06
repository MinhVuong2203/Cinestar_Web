using Web.Models;

namespace Web.Service
{
    public interface ICinemaBranchService
    {
        public List<CinemaBranch> GetBranches();
        //lấy danh sách các thành phố có rạp
        public List<string> GetListCityBranches();
        //lấy danh sách rạp theo thành phố
        public List<CinemaBranch> GetBranchesByCity(string city);
        // Lấy danh sách rạp theo thành phố và có chiếu phim cụ thể
        public List<CinemaBranch> GetBranchesByCityAndMovie(string city, string movieId);
    }
}
