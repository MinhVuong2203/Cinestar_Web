using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters
{
    public class AreaPrefixFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";

            // Kiểm tra user đã đăng nhập chưa
            if (!user.Identity?.IsAuthenticated ?? true)
                return;

            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            // Xác định prefix đúng theo role
            string correctPrefix = role switch
            {
                "Admin" => "/admin",
                "EmployeeSales" => "/employee-sales",
                "EmployeeTechnician" => "/employee-technician",
                "EmployeeMovies" => "/employee-movies",
                _ => "/admin"
            };

            // Nếu đang truy cập /Admin (Area) mà không đúng prefix
            if (path.StartsWith("/admin") && !path.StartsWith(correctPrefix))
            {
                var newPath = path.Replace("/admin", correctPrefix);
                context.Result = new RedirectResult(newPath);
                return;
            }

            // Redirect các employee nếu cố truy cập prefix của nhau
            if (role != "Admin")
            {
                if ((role == "EmployeeSales" && (path.StartsWith("/employee-technician") || path.StartsWith("/employee-movies"))) ||
                    (role == "EmployeeTechnician" && (path.StartsWith("/employee-sales") || path.StartsWith("/employee-movies"))) ||
                    (role == "EmployeeMovies" && (path.StartsWith("/employee-sales") || path.StartsWith("/employee-technician"))))
                {
                    context.Result = new RedirectResult(correctPrefix + "/Home/Index");
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
