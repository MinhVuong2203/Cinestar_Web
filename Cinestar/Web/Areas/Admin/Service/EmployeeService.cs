using Microsoft.EntityFrameworkCore;
using System.Data;
using Web.Data;
using Web.Models;

namespace Web.Areas.Admin.Service
{
    public class EmployeeService : IEmployeeService
    {
        private readonly CineStarContext _context;
        public EmployeeService(CineStarContext context)
        {
            this._context = context;
        }

        public async Task<bool> IsPhoneExist(string phone, Guid? excludeEmployeeId = null)
        {
            if (string.IsNullOrEmpty(phone)) return false;

            return await _context.Employees
                .AnyAsync(e => e.Phone == phone &&
                              !e.IsDeleted &&
                              e.EmployeeID != excludeEmployeeId);
        }

        public async Task<bool> IsEmailExist(string email, Guid? excludeEmployeeId = null)
        {
            email = email.ToLower();
            if (string.IsNullOrEmpty(email)) return false;
            return await _context.Employees
                .AnyAsync(e => e.Email == email &&
                              !e.IsDeleted &&
                              e.EmployeeID != excludeEmployeeId);
        }

        public async Task<bool> IsCCCDExist(string cccd, Guid? excludeEmployeeId = null)
        {
            if (string.IsNullOrEmpty(cccd)) return false;
            return await _context.Employees
                .AnyAsync(e => e.CCCD == cccd &&
                              !e.IsDeleted &&
                              e.EmployeeID != excludeEmployeeId);
        }

        public async Task<bool> IsUsernamexist(string username, Guid? excludeEmployeeId = null)
        {
            if (string.IsNullOrEmpty(username)) return false;
            return await _context.Employees
                .AnyAsync(e => e.Username == username &&
                              !e.IsDeleted &&
                              e.EmployeeID != excludeEmployeeId);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployees()
        {
            return await _context.Employees.Where(e => !e.IsDeleted).Include(e => e.Branch).ToListAsync();
        }

        public async Task<Employee?> GetEmployeeById(Guid id)
        {
            return await _context.Employees.Include(e => e.Branch).FirstOrDefaultAsync(e => !e.IsDeleted && e.EmployeeID == id);
        }

        public bool CheckBirthDate(DateOnly? birthDate)
        {
            if (birthDate == null)
                return false;
            var today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - birthDate.Value.Year; 
            if (birthDate.Value > today.AddYears(-age))
                age--;
            return age >= 18;
        }



        public async Task<bool> CreateEmployee(Employee employee)
        {
            try
            {
                employee.Email = employee.Email.ToLower();
                employee.RegisterDate = DateOnly.FromDateTime(DateTime.Now);
                employee.IsDeleted = false;
                await _context.Employees.AddAsync(employee);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }         
        }

        public async Task<bool> UpdateEmployee(Employee employee)
        {
            try
            {
                var trackedEntity = _context.ChangeTracker.Entries<Employee>()
                .FirstOrDefault(e => e.Entity.EmployeeID == employee.EmployeeID);

                if (trackedEntity != null)
                {
                    trackedEntity.State = EntityState.Detached;
                }
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }

        }
        //}
        //public Task<bool> DeleteEmployee(Guid id) // Soft delete{
        //{

            //} 

        public async Task<IEnumerable<string>> GetAllRoles()
        {
            return await _context.Employees.Select(e => e.Role).ToListAsync();
        }
    }
}
