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
                var customer = _context.Customers.AsNoTracking()
                    .FirstOrDefault(c =>
                        c.Username == username
                        && c.IsDeleted == false);
                // Verify password với BCrypt hash đã lưu
                if (customer == null || string.IsNullOrEmpty(customer.PasswordHash))
                {
                    return null;
                }

                // Kiểm tra password bằng BCrypt.Verify
                if (!BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
                {
                    return null;
                }

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

        public Customer? createCustomer(string fullname, string phone, string email, string username, string password, DateOnly birthDate)
        {
            try
            {
                // Kiểm tra username đã tồn tại
                var existingUsername = _context.Customers.AsNoTracking()
                    .Any(c => c.Username == username && !c.IsDeleted);
                if (existingUsername)
                {
                    return null; // Username đã tồn tại
                }

                // Kiểm tra email đã tồn tại
                var existingEmail = _context.Customers.AsNoTracking()
                    .Any(c => c.Email == email && !c.IsDeleted);
                if (existingEmail)
                {
                    return null; // Email đã tồn tại
                }

                // Kiểm tra phone đã tồn tại
                var existingPhone = _context.Customers.AsNoTracking()
                    .Any(c => c.Phone == phone && !c.IsDeleted);
                if (existingPhone)
                {
                    return null; // Phone đã tồn tại
                }

                // Hash password bằng BCrypt (để bảo mật tốt hơn)
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Tạo customer mới - BỎ CCCD
                var newCustomer = new Customer
                {
                    CustomerID = Guid.NewGuid(),
                    FullName = fullname,
                    Phone = phone,
                    Email = email,
                    Username = username,
                    PasswordHash = passwordHash,
                    BirthDate = birthDate,
                    RegisterDate = DateOnly.FromDateTime(DateTime.Now),
                    Point = 0,
                    VipLevel = 0,
                    IsDeleted = false
                };

                _context.Customers.Add(newCustomer);
                _context.SaveChanges();

                return newCustomer;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}