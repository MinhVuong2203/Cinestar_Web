using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Service
{
    public class Login : ILogin
    {
        private readonly CineStarContext _context;

        public Login(CineStarContext context)
        {
            _context = context;
        }

        public Customer? loginCustomer(string username, string password)
        {
            try
            {
                //var passHash = password;
                var customer = _context.Customers.AsNoTracking()
                    .FirstOrDefault(c =>
                        (c.Username == username || c.Email == username || c.Phone == username)
                        && c.PasswordHash == password
                        && c.IsDeleted == false);

                return customer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Employee? loginEmployee(string username, string password)
        {
            try
            {
                // Tìm employee theo username
                var employee = _context.Employees.AsNoTracking()
                    .FirstOrDefault(e =>
                        e.Username == username
                        && e.IsDeleted == false);

                // Verify password với BCrypt hash đã lưu
                if (employee == null || string.IsNullOrEmpty(employee.PasswordHash))
                {
                    return null;
                }

                // Kiểm tra password bằng BCrypt.Verify
                if (!BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash))
                {
                    return null;
                }

                return employee;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}