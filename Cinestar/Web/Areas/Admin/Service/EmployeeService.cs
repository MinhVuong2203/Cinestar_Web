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
                employee.Email = string.IsNullOrEmpty(employee.Email) ? null : employee.Email.ToLower();
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

        public async Task<IEnumerable<string>> GetAllRoles()
        {
            return await _context.Employees.Select(e => e.Role).Distinct().ToListAsync();
        }

        public async Task DeteleEmployee(Guid id)
        {
            Employee employee = await GetEmployeeById(id);
            employee.IsDeleted = true;
            _context.SaveChangesAsync();
        }


        public double GetSalaryById(Guid id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return 0;
            return emp.HourWage ?? 0; // nếu null thì trả về 0
        }

        // Lấy danh sách phim theo chi nhánh của nhân viên
        public List<Movie> GetMoviesByEmployeeBranchId(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
                return new List<Movie>();

            // Lấy danh sách phim đang chiếu tại chi nhánh thông qua ShowTime và Room
            var movies = _context.Movies
                .Where(m => !m.IsDeleted &&
                        m.ShowTimes.Any(st => !st.IsDeleted &&
                                        st.Room != null &&
                                        !st.Room.IsDeleted &&
                                        st.Room.BranchID == branchId))
                .Distinct()
                .ToList();

            return movies;
        }

        // Lấy thông tin loại vé và giá của phim tại chi nhánh
        public dynamic GetTicketTypesAndPrices(string movieId, string branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(movieId) || string.IsNullOrEmpty(branchId))
                    return null;

                // Debug: Log thông tin đầu vào
                Console.WriteLine($"Getting ticket types for MovieId: {movieId}, BranchId: {branchId}");

                // Lấy giá cơ bản từ ShowTime của phim tại chi nhánh
                var basePrice = _context.ShowTimes
                    .Include(st => st.Room)  // Include Room để đảm bảo có thể truy cập Room.BranchID
                    .Where(st => st.MovieID == movieId &&
                               !st.IsDeleted &&
                               st.Room != null &&
                               !st.Room.IsDeleted &&
                               st.Room.BranchID == branchId)
                    .Select(st => st.Price)
                    .FirstOrDefault();

                // Debug: Log base price
                Console.WriteLine($"Base price found: {basePrice}");

                // Nếu không tìm thấy, thử với giá mặc định dựa trên cấu hình rạp
                if (basePrice == null || basePrice == 0)
                {
                    // Kiểm tra xem phim có tồn tại không
                    var movieExists = _context.Movies.Any(m => m.MovieID == movieId && !m.IsDeleted);
                    var branchExists = _context.CinemaBranches.Any(b => b.BranchID == branchId && !b.IsDeleted);

                    Console.WriteLine($"Movie exists: {movieExists}, Branch exists: {branchExists}");

                    if (!movieExists || !branchExists)
                        return null;

                    // Sử dụng giá mặc định
                    basePrice = 80000;
                    Console.WriteLine($"Using default price: {basePrice}");
                }

                // Tạo danh sách loại vé với giá tương ứng
                var ticketTypes = new
                {
                    Standard = new { Name = "Vé thường", Description = "Ghế thường", Price = basePrice, Icon = "fas fa-ticket-alt" },
                    VIP = new { Name = "Vé VIP", Description = "Ghế VIP cao cấp", Price = basePrice + 20000, Icon = "fas fa-crown" },
                    Couple = new { Name = "Vé đôi", Description = "Ghế đôi couple", Price = (basePrice * 2) + 20000, Icon = "fas fa-heart" }
                };

                Console.WriteLine($"Returning ticket types: {Newtonsoft.Json.JsonConvert.SerializeObject(ticketTypes)}");

                return ticketTypes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTicketTypesAndPrices: {ex.Message}");
                return null;
            }
        }

    }
}
