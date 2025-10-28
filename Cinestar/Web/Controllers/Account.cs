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
                if (employee != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, employee.FullName ?? ""),
                        new Claim(ClaimTypes.Email, employee.Email ?? ""),
                        new Claim(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
                        new Claim("Username", employee.Username ?? ""),
                        new Claim(ClaimTypes.Role, employee.Role ?? "Employee"),
                        new Claim("UserType", "Employee")
                    };

                    var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = remember,
                        ExpiresUtc = remember ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimIdentity), authProperties);

                    string redirectUrl = employee.Role?.ToLower() == "admin" ? "/Admin" : "/Employee";

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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}