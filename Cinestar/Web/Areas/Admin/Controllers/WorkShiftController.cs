using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Service;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WorkShiftController : Controller
    {
        private readonly IWorkShiftService _workShiftService;
        private readonly IBranchService _branchService;
        private readonly IEmployeeService _employeeService;

        public WorkShiftController(IWorkShiftService workShiftService, IBranchService branchService, IEmployeeService employeeService)
        {
            _workShiftService = workShiftService;
            _branchService = branchService;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index(string branchId, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Branches = await _branchService.GetCinemaBranches();
            ViewBag.CurrentBranch = branchId;

            fromDate ??= DateTime.Today;
            toDate ??= fromDate.Value.AddDays(6);

            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(branchId))
                return View(null);

            var employees = await _workShiftService.GetEmployeesByBranch(branchId);
            var shifts = await _workShiftService.GetWorkShifts(branchId, fromDate.Value, toDate.Value);

            var dates = Enumerable.Range(0, (toDate.Value - fromDate.Value).Days + 1)
                .Select(offset => fromDate.Value.AddDays(offset))
                .ToList();

            var vm = new ShiftMatrixVM
            {
                Employees = employees,
                Dates = dates,
                Shifts = shifts
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssignShift(Guid employeeId, string branchId, DateTime date, string slot)
        {
            TimeSpan start, end;
            switch (slot)
            {
                case "S": start = new(8, 0, 0); end = new(12, 0, 0); break;
                case "C": start = new(12, 0, 0); end = new(16, 0, 0); break;
                default: start = new(16, 0, 0); end = new(23, 0, 0); break;
            }

            var startTime = date.Date.Add(start);
            var endTime = date.Date.Add(end);

            var shift = new WorkShift
            {
                EmployeeID = employeeId,
                BranchID = branchId,
                StartTime = startTime,
                EndTime = endTime,
                SalaryPerHour = (decimal?) (_employeeService.GetSalaryById(employeeId) * (endTime - startTime).TotalHours),
                Status = "Sắp làm"
            };

            await _workShiftService.CreateWorkShift(shift);
            return Json(new { success = true, message = "Đã xếp ca thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveShift(string shiftId)
        {
            var ok = await _workShiftService.DeleteWorkShift(shiftId);
            return Json(new { success = ok });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatusShift(string shiftId, string status)
        {
            var ok = await _workShiftService.UpdateStatus(shiftId, status);
            return Json(new { success = ok });
        }

    }

    public class ShiftMatrixVM
    {
        public List<Employee> Employees { get; set; } = new();
        public List<DateTime> Dates { get; set; } = new();
        public List<WorkShift> Shifts { get; set; } = new();
    }
}
