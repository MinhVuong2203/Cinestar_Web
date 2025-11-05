using ClosedXML.Excel;
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

        public async Task<IActionResult> Index(string branchId, string role, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Branches = await _branchService.GetCinemaBranches();
            ViewBag.CurrentBranch = branchId;
            ViewBag.CurrentRole = role;

            fromDate ??= DateTime.Today;
            toDate ??= fromDate.Value.AddDays(6);
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(branchId))
                return View(null);

            var employees = await _workShiftService.GetEmployeesByBranch(branchId);

            // Lọc theo chức vụ nếu có
            if (!string.IsNullOrEmpty(role))
            {
                employees = employees.Where(e => e.Role?.Trim().Equals(role, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

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
                SalaryPerHour = (decimal?)(_employeeService.GetSalaryById(employeeId) * (endTime - startTime).TotalHours),
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

        public async Task<IActionResult> ExportExcel(string branchId, string role, DateTime? fromDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(branchId))
                return BadRequest("Vui lòng chọn chi nhánh");

            fromDate ??= DateTime.Today;
            toDate ??= fromDate.Value.AddDays(6);

            var employees = await _workShiftService.GetEmployeesByBranch(branchId);

            // Lọc theo chức vụ nếu có
            if (!string.IsNullOrEmpty(role))
            {
                employees = employees.Where(e => e.Role?.Trim().Equals(role, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            var shifts = await _workShiftService.GetWorkShifts(branchId, fromDate.Value, toDate.Value);
            var dates = Enumerable.Range(0, (toDate.Value - fromDate.Value).Days + 1)
                .Select(offset => fromDate.Value.AddDays(offset))
                .ToList();

            var branches = await _branchService.GetCinemaBranches();
            var branchName = branches.FirstOrDefault(b => b.BranchID == branchId)?.BranchName ?? "Unknown";

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Lịch phân ca");

            // Tiêu đề
            worksheet.Cell(1, 1).Value = "LỊCH PHÂN CA LÀM VIỆC";
            worksheet.Range(1, 1, 1, dates.Count * 3 + 1).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            worksheet.Cell(2, 1).Value = $"Chi nhánh: {branchName}";
            worksheet.Range(2, 1, 2, dates.Count * 3 + 1).Merge().Style
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            worksheet.Cell(3, 1).Value = $"Từ ngày {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}";
            worksheet.Range(3, 1, 3, dates.Count * 3 + 1).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Header - Dòng 1: Ngày
            int col = 2;
            worksheet.Cell(4, 1).Value = "Nhân viên";
            foreach (var date in dates)
            {
                worksheet.Cell(4, col).Value = date.ToString("dd/MM");
                worksheet.Range(4, col, 4, col + 2).Merge();
                col += 3;
            }

            // Header - Dòng 2: Ca
            col = 2;
            foreach (var date in dates)
            {
                worksheet.Cell(5, col).Value = "Sáng";
                worksheet.Cell(5, col + 1).Value = "Chiều";
                worksheet.Cell(5, col + 2).Value = "Tối";
                col += 3;
            }

            // Style cho header
            var headerRange = worksheet.Range(4, 1, 5, dates.Count * 3 + 1);
            headerRange.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            // Dữ liệu nhân viên
            int row = 6;
            foreach (var emp in employees)
            {
                worksheet.Cell(row, 1).Value = $"{emp.FullName}\n({emp.Role})";
                worksheet.Cell(row, 1).Style.Alignment.WrapText = true;

                col = 2;
                foreach (var date in dates)
                {
                    foreach (var slot in new[] { "S", "C", "T" })
                    {
                        var timeStart = slot == "S" ? new TimeSpan(8, 0, 0) :
                                       slot == "C" ? new TimeSpan(12, 0, 0) :
                                       new TimeSpan(16, 0, 0);

                        var shift = shifts.FirstOrDefault(s =>
                            s.EmployeeID == emp.EmployeeID &&
                            s.StartTime.Date == date.Date &&
                            s.StartTime.TimeOfDay == timeStart);

                        if (shift != null)
                        {
                            worksheet.Cell(row, col).Value = $"{shift.StartTime:HH:mm}-{shift.EndTime:HH:mm}\n{shift.Status}";
                            worksheet.Cell(row, col).Style.Alignment.WrapText = true;

                            // Màu sắc theo trạng thái
                            var cellColor = shift.Status?.ToLower().Trim() switch
                            {
                                "đang làm" => XLColor.LightBlue,
                                "hoàn thành" => XLColor.LightGreen,
                                "vắng" => XLColor.LightPink,
                                "nghỉ phép" => XLColor.LightYellow,
                                "sắp làm" => XLColor.Lavender,
                                _ => XLColor.White
                            };
                            worksheet.Cell(row, col).Style.Fill.SetBackgroundColor(cellColor);
                        }
                        else
                        {
                            worksheet.Cell(row, col).Value = "—";
                        }

                        col++;
                    }
                }
                row++;
            }

            // Style cho bảng dữ liệu
            var dataRange = worksheet.Range(6, 1, row - 1, dates.Count * 3 + 1);
            dataRange.Style
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            // Cột nhân viên căn trái
            worksheet.Column(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Column(1).Width = 25;

            // Các cột ca làm việc
            for (int i = 2; i <= dates.Count * 3 + 1; i++)
            {
                worksheet.Column(i).Width = 12;
            }

            // Xuất file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var fileName = $"LichPhanCa_{branchName}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> SalaryReport(string branchId, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Branches = await _branchService.GetCinemaBranches();
            ViewBag.CurrentBranch = branchId;

            fromDate ??= DateTime.Today.AddMonths(-1);
            toDate ??= DateTime.Today;

            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(branchId))
                return View(null);

            var employees = await _workShiftService.GetEmployeesByBranch(branchId);
            var shifts = await _workShiftService.GetWorkShifts(branchId, fromDate.Value, toDate.Value);

            var employeeStats = employees.Select(emp => new EmployeeSalaryStatVM
            {
                EmployeeID = emp.EmployeeID,
                FullName = emp.FullName,
                Role = emp.Role,
                CompletedCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Hoàn thành"),
                WorkingCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Đang làm"),
                AbsentCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Vắng"),
                LeaveCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Nghỉ phép"),
                TotalSalary = shifts
                    .Where(s => s.EmployeeID == emp.EmployeeID && (s.Status == "Hoàn thành" || s.Status == "Đang làm"))
                    .Sum(s => s.SalaryPerHour ?? 0)
            }).ToList();

            var vm = new SalaryReportVM
            {
                BranchId = branchId,
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                EmployeeStats = employeeStats,
                TotalCompleted = employeeStats.Sum(e => e.CompletedCount),
                TotalWorking = employeeStats.Sum(e => e.WorkingCount),
                TotalAbsent = employeeStats.Sum(e => e.AbsentCount),
                TotalLeave = employeeStats.Sum(e => e.LeaveCount),
                TotalShifts = employeeStats.Sum(e => e.TotalShifts),
                TotalSalary = employeeStats.Sum(e => e.TotalSalary)
            };

            return View(vm);
        }

        //public async Task<IActionResult> ExportExcel(string branchId, DateTime? fromDate, DateTime? toDate)
        //{
        //    if (string.IsNullOrEmpty(branchId))
        //        return BadRequest("Vui lòng chọn chi nhánh");

        //    fromDate ??= DateTime.Today.AddMonths(-1);
        //    toDate ??= DateTime.Today;

        //    var employees = await _workShiftService.GetEmployeesByBranch(branchId);
        //    var shifts = await _workShiftService.GetWorkShifts(branchId, fromDate.Value, toDate.Value);

        //    var employeeStats = employees.Select(emp => new EmployeeSalaryStatVM
        //    {
        //        EmployeeID = emp.EmployeeID,
        //        FullName = emp.FullName,
        //        Role = emp.Role,
        //        CompletedCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Hoàn thành"),
        //        WorkingCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Đang làm"),
        //        AbsentCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Vắng"),
        //        LeaveCount = shifts.Count(s => s.EmployeeID == emp.EmployeeID && s.Status == "Nghỉ phép"),
        //        TotalSalary = shifts
        //            .Where(s => s.EmployeeID == emp.EmployeeID && (s.Status == "Hoàn thành" || s.Status == "Đang làm"))
        //            .Sum(s => s.SalaryPerHour ?? 0)
        //    }).OrderByDescending(e => e.TotalSalary).ToList();

        //    var branches = await _branchService.GetCinemaBranches();
        //    var branchName = branches.FirstOrDefault(b => b.BranchID == branchId)?.BranchName ?? "Unknown";

        //    using var workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("Báo cáo lương");

        //    // Tiêu đề
        //    worksheet.Cell(1, 1).Value = "BÁO CÁO LƯƠNG VÀ HIỆU SUẤT LÀM VIỆC";
        //    worksheet.Range(1, 1, 1, 9).Merge().Style
        //        .Font.SetBold().Font.SetFontSize(16)
        //        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        //    worksheet.Cell(2, 1).Value = $"Chi nhánh: {branchName}";
        //    worksheet.Range(2, 1, 2, 9).Merge().Style
        //        .Font.SetBold()
        //        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        //    worksheet.Cell(3, 1).Value = $"Từ ngày {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}";
        //    worksheet.Range(3, 1, 3, 9).Merge().Style
        //        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        //    // Header
        //    var headers = new[] { "STT", "Họ tên", "Chức vụ", "Hoàn thành", "Đang làm", "Vắng", "Nghỉ phép", "Tổng ca", "Lương (đ)" };
        //    for (int i = 0; i < headers.Length; i++)
        //    {
        //        worksheet.Cell(5, i + 1).Value = headers[i];
        //    }

        //    var headerRange = worksheet.Range(5, 1, 5, 9);
        //    headerRange.Style
        //        .Font.SetBold()
        //        .Fill.SetBackgroundColor(XLColor.LightBlue)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Border.SetInsideBorder(XLBorderStyleValues.Thin)
        //        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        //    // Dữ liệu
        //    int row = 6;
        //    int index = 1;
        //    foreach (var emp in employeeStats)
        //    {
        //        worksheet.Cell(row, 1).Value = index;
        //        worksheet.Cell(row, 2).Value = emp.FullName;
        //        worksheet.Cell(row, 3).Value = emp.Role;
        //        worksheet.Cell(row, 4).Value = emp.CompletedCount;
        //        worksheet.Cell(row, 5).Value = emp.WorkingCount;
        //        worksheet.Cell(row, 6).Value = emp.AbsentCount;
        //        worksheet.Cell(row, 7).Value = emp.LeaveCount;
        //        worksheet.Cell(row, 8).Value = emp.TotalShifts;
        //        worksheet.Cell(row, 9).Value = emp.TotalSalary;

        //        // Màu sắc theo số lần vắng
        //        if (emp.AbsentCount > 3)
        //        {
        //            worksheet.Cell(row, 6).Style.Fill.SetBackgroundColor(XLColor.LightPink);
        //        }

        //        row++;
        //        index++;
        //    }

        //    // Tổng cộng
        //    worksheet.Cell(row, 1).Value = "TỔNG CỘNG";
        //    worksheet.Range(row, 1, row, 3).Merge().Style.Font.SetBold();
        //    worksheet.Cell(row, 4).Value = employeeStats.Sum(e => e.CompletedCount);
        //    worksheet.Cell(row, 5).Value = employeeStats.Sum(e => e.WorkingCount);
        //    worksheet.Cell(row, 6).Value = employeeStats.Sum(e => e.AbsentCount);
        //    worksheet.Cell(row, 7).Value = employeeStats.Sum(e => e.LeaveCount);
        //    worksheet.Cell(row, 8).Value = employeeStats.Sum(e => e.TotalShifts);
        //    worksheet.Cell(row, 9).Value = employeeStats.Sum(e => e.TotalSalary);

        //    var totalRange = worksheet.Range(row, 1, row, 9);
        //    totalRange.Style
        //        .Font.SetBold()
        //        .Fill.SetBackgroundColor(XLColor.LightGray)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        //    // Style cho bảng dữ liệu
        //    var dataRange = worksheet.Range(6, 1, row, 9);
        //    dataRange.Style
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Border.SetInsideBorder(XLBorderStyleValues.Thin)
        //        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        //    // Căn trái cột họ tên
        //    worksheet.Column(2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        //    // Format số tiền
        //    worksheet.Column(9).Style.NumberFormat.Format = "#,##0";
        //    worksheet.Column(9).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        //    // Độ rộng cột
        //    worksheet.Column(1).Width = 8;
        //    worksheet.Column(2).Width = 30;
        //    worksheet.Column(3).Width = 20;
        //    worksheet.Column(4).Width = 12;
        //    worksheet.Column(5).Width = 12;
        //    worksheet.Column(6).Width = 12;
        //    worksheet.Column(7).Width = 12;
        //    worksheet.Column(8).Width = 12;
        //    worksheet.Column(9).Width = 18;

        //    // Xuất file
        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    var content = stream.ToArray();

        //    var fileName = $"BaoCaoLuong_{branchName}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
        //    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //}

    }
        public class ShiftMatrixVM
        {
            public List<Employee> Employees { get; set; } = new();
            public List<DateTime> Dates { get; set; } = new();
            public List<WorkShift> Shifts { get; set; } = new();
        }

    public class SalaryReportVM
    {
        public string BranchId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<EmployeeSalaryStatVM> EmployeeStats { get; set; } = new();
        public int TotalCompleted { get; set; }
        public int TotalWorking { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLeave { get; set; }
        public int TotalShifts { get; set; }
        public decimal TotalSalary { get; set; }
    }

    public class EmployeeSalaryStatVM
    {
        public Guid EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public int CompletedCount { get; set; }
        public int WorkingCount { get; set; }
        public int AbsentCount { get; set; }
        public int LeaveCount { get; set; }
        public int TotalShifts => CompletedCount + WorkingCount + AbsentCount + LeaveCount;
        public decimal TotalSalary { get; set; }
    }
}