using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class WorkShiftService : IWorkShiftService
    {
        private readonly CineStarContext _context;

        public WorkShiftService(CineStarContext context)
        {
            _context = context;
        }
        public async Task<List<Employee>> GetEmployeesByBranch(string branchId)
        {
            return await _context.Employees
                .Where(e => e.BranchID == branchId && !e.IsDeleted)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<List<WorkShift>> GetWorkShifts(string branchId, DateTime fromDate, DateTime toDate)
        {
            return await _context.WorkShifts
                .Include(ws => ws.Employee)
                .Where(ws => ws.BranchID == branchId &&
                             ws.StartTime.Date >= fromDate.Date &&
                             ws.StartTime.Date <= toDate.Date)
                .ToListAsync();
        }

        public async Task<bool> CreateWorkShift(WorkShift shift)
        {
            _context.WorkShifts.Add(shift);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteWorkShift(string shiftId)
        {
            var ws = await _context.WorkShifts.FindAsync(shiftId);
            if (ws == null) return false;
            _context.WorkShifts.Remove(ws);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatus(string shiftId, string status)
        {
            var ws = await _context.WorkShifts.FindAsync(shiftId);
            if (ws == null) return false;
            ws.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public double CalculateWorkingHours(DateTime start, DateTime end)
        {
            return Math.Round((end - start).TotalHours, 2);
        }
    }
}
