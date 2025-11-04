using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface IWorkShiftService
    {
        Task<List<Employee>> GetEmployeesByBranch(string branchId);
        Task<List<WorkShift>> GetWorkShifts(string branchId, DateTime fromDate, DateTime toDate);
        Task<bool> CreateWorkShift(WorkShift shift);
        Task<bool> DeleteWorkShift(string shiftId);
        Task<bool> UpdateStatus(string shiftId, string status);
        double CalculateWorkingHours(DateTime start, DateTime end);



    }
}
