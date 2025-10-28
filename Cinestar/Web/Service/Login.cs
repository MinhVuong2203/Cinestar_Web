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
                var employee = _context.Employees.AsNoTracking()
                    .FirstOrDefault(e =>
                        (e.Username == username || e.Email == username || e.Phone == username)
                        && e.PasswordHash == password
                        && e.IsDeleted == false);

                return employee;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}