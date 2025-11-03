using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Web.Areas.Admin.Service;
using Web.Models;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IBranchService _branchService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeeController(IEmployeeService employeeService, IBranchService branchService , IWebHostEnvironment webHostEnvironment)
        {
            _employeeService = employeeService;
            _branchService = branchService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> CheckPhone(string phone, Guid? employeeID)
        {
            if (string.IsNullOrEmpty(phone))
                return Json(true);

            var exists = await _employeeService.IsPhoneExist(phone, employeeID);
            return Json(!exists);
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email, Guid? employeeID)
        {
            if (string.IsNullOrEmpty(email))
                return Json(true);

            var exists = await _employeeService.IsEmailExist(email, employeeID);
            return Json(!exists);
        }

        [HttpGet]
        public async Task<IActionResult> CheckCCCD(string cccd, Guid? employeeID)
        {
            if (string.IsNullOrEmpty(cccd))
                return Json(true);

            var exists = await _employeeService.IsCCCDExist(cccd, employeeID);
            return Json(!exists);
        }

        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username, Guid? employeeID)
        {
            if (string.IsNullOrEmpty(username))
                return Json(true);

            var exists = await _employeeService.IsUsernamexist(username, employeeID);
            return Json(!exists);
        }

        [HttpGet]
        public IActionResult CheckBirthDate(DateOnly? BirthDate)
        {
            if (BirthDate == null)
                return Json(true);
            bool isAdult = _employeeService.CheckBirthDate(BirthDate);
            return Json(isAdult);
        }




        // GET: Employee/Index
        public async Task<IActionResult> Index(string branchId, string searchTerm, string role, string sortBy)
        {
            IEnumerable<Employee> employees;
            employees = await _employeeService.GetAllEmployees();

            // Lọc theo chi nhánh
            if (!string.IsNullOrEmpty(branchId)){
                employees = employees.Where(e => e.BranchID == branchId);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                employees = employees.Where(e =>
                    e.FullName.ToLower().Contains(searchTerm) ||
                    (e.Email != null && e.Email.ToLower().Contains(searchTerm)) ||
                    (e.Phone != null && e.Phone.Contains(searchTerm)) ||
                    e.Role.ToLower().Contains(searchTerm) ||
                    (e.CCCD != null && e.CCCD.Contains(searchTerm))
                );
            }

            // Lọc theo chức vụ
            if (!string.IsNullOrEmpty(role))
            {
                employees = employees.Where(e => e.Role == role);
            }
            // Sắp xếp
            employees = sortBy switch
            {
                "name_asc" => employees.OrderBy(e => e.FullName),
                "name_desc" => employees.OrderByDescending(e => e.FullName),
                "date_asc" => employees.OrderBy(e => e.RegisterDate),
                "date_desc" => employees.OrderByDescending(e => e.RegisterDate),
                _ => employees.OrderByDescending(e => e.RegisterDate)
            };

            // Lấy danh sách chức vụ cho filter
            ViewBag.Roles = await _employeeService.GetAllRoles();
            ViewBag.Branches = await _branchService.GetCinemaBranches();
            ViewBag.CurrentBranch = branchId;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentRole = role;
            ViewBag.CurrentSort = sortBy;


            return View(employees);
        }

        // Employee/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Branches = await _branchService.GetCinemaBranches();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Upload ảnh
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "image", "employee");
                        Directory.CreateDirectory(uploadPath);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        employee.ImageUrl = $"/image/employee/{fileName}";
                    }
                    employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(employee.PasswordHash);
                    var result = await _employeeService.CreateEmployee(employee);
                    if (result)
                    {
                        TempData["Success"] = "Thêm nhân viên thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateException ex)
                {
                    // Bắt lỗi UNIQUE constraint từ database (backup validation)
                    if (ex.InnerException?.Message.Contains("UQ__Employee__5C7E359E2F86D62D") == true)
                    {
                        ModelState.AddModelError(nameof(employee.Phone), "Số điện thoại đã tồn tại");
                    }
                    else if (ex.InnerException?.Message.Contains("UQ__Employee__A9D10534B9BFB939") == true)
                    {
                        ModelState.AddModelError(nameof(employee.Email), "Email đã tồn tại");
                    }
                    else if (ex.InnerException?.Message.Contains("UQ__Employee__A955A0AA3E66A65F") == true)
                    {
                        ModelState.AddModelError(nameof(employee.CCCD), "CCCD đã tồn tại");
                    }
                    else
                    {
                        TempData["Error"] = "Có lỗi xảy ra khi thêm nhân viên!";
                    }
                }
            }

            return View(employee);
        }

        // GET: Employee/Edit/id
        public async Task<IActionResult> Edit(Guid id)
        {
            var employee = await _employeeService.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewBag.Branches = await _branchService.GetCinemaBranches();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Employee employee, IFormFile? ImageFile)
        {
            if (id != employee.EmployeeID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                Employee existingEmployee = await _employeeService.GetEmployeeById(id);
                if (existingEmployee == null)
                {
                    return NotFound();
                }
                try
                {
                    // Upload ảnh
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "image", "employee");
                        Directory.CreateDirectory(uploadPath);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        employee.ImageUrl = $"/image/employee/{fileName}";
                    }
                    else
                    {   
                        if (existingEmployee != null)
                        {
                            employee.ImageUrl = existingEmployee.ImageUrl;
                        }
                    }

                    // Xử lý password
                    if (!string.IsNullOrEmpty(employee.PasswordHash))
                    {
                        employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(employee.PasswordHash);
                    }
                    else
                    {
                        employee.PasswordHash = existingEmployee.PasswordHash;
                    }

                    var result = await _employeeService.UpdateEmployee(employee);
                    if (result)
                    {
                        TempData["Success"] = "Cập nhật nhân viên thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateException ex)
                {
                    // Bắt lỗi UNIQUE constraint từ database (backup validation)
                    if (ex.InnerException?.Message.Contains("UQ__Employee__5C7E359E2F86D62D") == true)
                    {
                        ModelState.AddModelError(nameof(employee.Phone), "Số điện thoại đã tồn tại");
                    }
                    else if (ex.InnerException?.Message.Contains("UQ__Employee__A9D10534B9BFB939") == true)
                    {
                        ModelState.AddModelError(nameof(employee.Email), "Email đã tồn tại");
                    }
                    else if (ex.InnerException?.Message.Contains("UQ__Employee__A955A0AA3E66A65F") == true)
                    {
                        ModelState.AddModelError(nameof(employee.CCCD), "CCCD đã tồn tại");
                    }
                    else
                    {
                        TempData["Error"] = "Có lỗi xảy ra khi thêm nhân viên!";
                    }
                }
            }

            return View(employee);
        }

    }
}
