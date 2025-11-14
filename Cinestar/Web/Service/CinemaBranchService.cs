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

        public List<string> GetListCityBranches()
        {
            try
            {
                return _context.CinemaBranches
                    .Where(b => !b.IsDeleted)
                    .Select(b => b.City)
                    .Distinct()
                    .OrderBy(city => city != "TP.HCM") // false (HCM) sẽ lên đầu
                    .ThenBy(city => city)
                    .AsNoTracking()
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
        //lấy danh sách rạp theo thành phố
        public List<CinemaBranch> GetBranchesByCity(string city)
        {
            try
            {
                return _context.CinemaBranches
                    .Where(b => !b.IsDeleted && b.City == city)
                    .AsNoTracking()
                    .ToList();
            }
            catch
            {
                return new List<CinemaBranch>();
            }
        }

        // Lấy danh sách rạp theo thành phố và có chiếu phim cụ thể
        public List<CinemaBranch> GetBranchesByCityAndMovie(string city, string movieId)
        {
            try
            {
                return _context.CinemaBranches
                    .Where(b => !b.IsDeleted && b.City == city)
                    .Where(b => b.Rooms.Any(r => !r.IsDeleted &&
                        r.ShowTimes.Any(st => !st.IsDeleted
                            && st.MovieID == movieId
                            && st.StartTime >= DateTime.Today))) // Chỉ lấy lịch chiếu từ hôm nay
                    .AsNoTracking()
                    .ToList();
            }
            catch
            {
                return new List<CinemaBranch>();
            }
        }
    }
}
