using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class ShowTimeService : IShowTimeService
    {
        private readonly CineStarContext _db;

        public ShowTimeService(CineStarContext db)
        {
            _db = db;
        }

        public async Task<List<ShowTime>> GetAllAsync()
            => await _db.ShowTimes.Include(x => x.Movie).Include(x => x.Room).ToListAsync();

        public async Task<ShowTime?> GetByIdAsync(string id)
            => await _db.ShowTimes.Include(x => x.Movie).Include(x => x.Room).FirstOrDefaultAsync(st => st.ShowTimeID == id);

        public async Task<bool> CreateAsync(ShowTime showTime)
        {
            try
            {
                _db.ShowTimes.Add(showTime); await _db.SaveChangesAsync(); return true;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateAsync(ShowTime showTime)
        {
            try
            {
                _db.ShowTimes.Update(showTime); await _db.SaveChangesAsync(); return true;
            }
            catch { return false; }
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            var st = await _db.ShowTimes.FindAsync(id);
            if (st == null) return false;
            st.IsDeleted = true; _db.ShowTimes.Update(st); await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(string id)
        {
            var st = await _db.ShowTimes.FindAsync(id);
            if (st == null) return false;
            st.IsDeleted = false; _db.ShowTimes.Update(st); await _db.SaveChangesAsync();
            return true;
        }
    }
}
