using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Web.Helper;
using Web.Service;

namespace Web.Controllers
{
    public class Account : Controller
    {
        private readonly ILogin _login;

        public Account(ILogin login)
        {
            _login = login;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult CinestartMember()
        {
            return View();
        }

        public IActionResult PurchaseHistory()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Login(string username, string password, bool remember = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return Json(new { status = webconstain.erro, message = "Vui lòng nhập đầy đủ thông tin đăng nhập" });
                }

                // Try customer login first
                var customer = _login.loginCustomer(username, password);
                if (customer != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, customer.FullName ?? ""),
                        new Claim(ClaimTypes.Email, customer.Email ?? ""),
                        new Claim(ClaimTypes.NameIdentifier, customer.CustomerID.ToString()),
                        new Claim("Username", customer.Username ?? ""),
                        new Claim(ClaimTypes.Role, "Customer"),
                        new Claim("UserType", "Customer")
                    };

                    var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = remember,
                        ExpiresUtc = remember ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimIdentity), authProperties);

                    return Json(new
                    {
                        status = webconstain.success,
                        message = "Đăng nhập thành công",
                        customerName = customer.FullName ?? "Khách hàng",
                        redirectUrl = "/",
                        userType = "Customer"
                    });
                }

                // Try employee login
                var employee = _login.loginEmployee(username, password);
                string role = "";
                if (employee.Role.Equals("Quản lý"))
                {
                    role = "Admin";
                }else if (employee.Role.Equals("Nhân viên kỹ thuật")) {
                    role = "EmployeeTechnician";
                }
                else if (employee.Role.Equals("Nhân viên bán vé")) {
                    role = "EmployeeSales";
                }
                else if (employee.Role.Equals("Nhân viên phim"))
                {
                    role = "EmployeeMovies";
                }
                if (employee != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, employee.FullName ?? ""),
                        new Claim(ClaimTypes.Email, employee.Email ?? ""),
                        new Claim(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
                        new Claim("Username", employee.Username ?? ""),
                        new Claim(ClaimTypes.Role, role ?? ""),
                        new Claim("UserType", "Employee")
                    };

                    var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    if (remember)
                    {
                        // Cho phép lưu đăng nhập
                        authProperties.IsPersistent = true;
                        authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
                    }
                    else
                    {
                        // Cookie session: tắt browser là mất
                        authProperties.IsPersistent = false;
                        authProperties.ExpiresUtc = null;
                    }


                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimIdentity), authProperties);

                    string redirectUrl = role switch
                    {
                        "Admin" => "/admin/Home/Index",
                        "EmployeeSales" => "/employee-sales/Home/Index",
                        "EmployeeTechnician" => "/employee-technician/Home/Index",
                        "EmployeeMovies" => "/employee-movies/Home/Index",
                        _ => "/admin/Home/Index"
                    };

                    return Json(new
                    {
                        status = webconstain.success,
                        message = "Đăng nhập thành công",
                        customerName = employee.FullName ?? "Nhân viên",
                        redirectUrl = redirectUrl,
                        userType = "Employee"
                    });
                }

                return Json(new
                {
                    status = webconstain.erro,
                    message = "Tên đăng nhập hoặc mật khẩu không đúng"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = webconstain.erro,
                    message = "Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại sau.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Register(string fullname, string birthday, string phone, string username, string email, string password)
        {
            try
            {
                // Validation - BỎ CCCD
                if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(birthday))
                {
                    return Json(new { status = webconstain.erro, message = "Vui lòng nhập đầy đủ thông tin" });
                }

                // Chuyển đổi birthday sang DateOnly
                if (!DateOnly.TryParse(birthday, out DateOnly birthDate))
                {
                    return Json(new { status = webconstain.erro, message = "Ngày sinh không hợp lệ" });
                }

                // Tạo customer mới - BỎ THAM SỐ CCCD
                var newCustomer = _login.createCustomer(fullname, phone, email, username, password, birthDate);

                if (newCustomer == null)
                {
                    return Json(new { status = webconstain.erro, message = "Tên đăng nhập, email hoặc số điện thoại đã tồn tại" });
                }

                // Tự động đăng nhập sau khi đăng ký thành công
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, newCustomer.FullName ?? ""),
                    new Claim(ClaimTypes.Email, newCustomer.Email ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, newCustomer.CustomerID.ToString()),
                    new Claim("Username", newCustomer.Username ?? ""),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("UserType", "Customer")
                };

                var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimIdentity), authProperties);

                return Json(new
                {
                    status = webconstain.success,
                    message = "Đăng ký thành công",
                    customerName = newCustomer.FullName,
                    redirectUrl = "/Account/Login"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = webconstain.erro,
                    message = "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại sau.",
                    error = ex.Message
                });
            }
        }

<<<<<<< Updated upstream
        [HttpPost]
        [ValidateAntiForgeryToken]
=======
        [HttpGet]
>>>>>>> Stashed changes
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Home/Index");

        }
    }
}