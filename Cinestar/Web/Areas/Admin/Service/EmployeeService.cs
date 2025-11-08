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

        // Lấy thông tin loại vé, số lượng vé available và giá của phim tại chi nhánh
        public dynamic GetTicketTypesAndPrices(string movieId, string branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(movieId) || string.IsNullOrEmpty(branchId))
                {
                    Console.WriteLine("ERROR: MovieId or BranchId is null/empty");
                    return null;
                }

                Console.WriteLine($"=== GetTicketTypesAndPrices ===");
                Console.WriteLine($"MovieId: {movieId}, BranchId: {branchId}");

                // Lấy danh sách suất chiếu của phim tại chi nhánh (từ hôm nay trở đi)
                var showTimeIds = _context.ShowTimes
                    .Include(st => st.Room)
                    .Where(st => st.MovieID == movieId &&
                               !st.IsDeleted &&
                               st.Room != null &&
                               !st.Room.IsDeleted &&
                               st.Room.BranchID == branchId &&
                               st.StartTime.Date == DateTime.Today)
                    .Select(st => st.ShowTimeID)
                    .ToList();

                if (!showTimeIds.Any())
                {
                    Console.WriteLine("WARNING: No showtimes found for this movie at this branch");
                    return null;
                }

                Console.WriteLine($"Found {showTimeIds.Count} showtimes");

                // Đếm số lượng vé available theo từng loại ghế (SeatType)
                var ticketStats = _context.Tickets
                    .Include(t => t.Seat)
                    .Where(t => showTimeIds.Contains(t.ShowTimeID) &&
                                !t.IsDeleted &&
                                t.Status == "Trống" &&
                                t.Seat != null &&
                                !t.Seat.IsDeleted)
                    .GroupBy(t => t.Seat.SeatType)
                    .Select(g => new
                    {
                        SeatType = g.Key,
                        AvailableCount = g.Count(),
                        Price = g.Min(t => t.Price ?? 0)  // ✅ THAY ĐỔI: Dùng Min thay vì Average
                    })
                    .ToList();

                Console.WriteLine($"Ticket statistics: {Newtonsoft.Json.JsonConvert.SerializeObject(ticketStats)}");

                // Nếu không có vé available nào
                if (!ticketStats.Any())
                {
                    Console.WriteLine("WARNING: No available tickets found");
                    return null;
                }

                // Tạo response object với thông tin chi tiết
                var standardTicket = ticketStats.FirstOrDefault(t => t.SeatType == "Ghế thường");
                var vipTicket = ticketStats.FirstOrDefault(t => t.SeatType == "Ghế VIP");
                var coupleTicket = ticketStats.FirstOrDefault(t => t.SeatType == "Ghế Couple");

                var result = new
                {
                    Standard = standardTicket != null
                        ? new
                        {
                            Name = "Vé thường",
                            Description = "Ghế thường",
                            Price = (decimal)standardTicket.Price,
                            AvailableCount = standardTicket.AvailableCount,
                            Icon = "fas fa-ticket-alt"
                        }
                        : null,
                    VIP = vipTicket != null
                        ? new
                        {
                            Name = "Vé VIP",
                            Description = "Ghế VIP cao cấp",
                            Price = (decimal)vipTicket.Price,
                            AvailableCount = vipTicket.AvailableCount,
                            Icon = "fas fa-crown"
                        }
                        : null,
                    Couple = coupleTicket != null
                        ? new
                        {
                            Name = "Vé đôi",
                            Description = "Ghế đôi couple",
                            Price = (decimal)coupleTicket.Price,
                            AvailableCount = coupleTicket.AvailableCount,
                            Icon = "fas fa-heart"
                        }
                        : null
                };

                Console.WriteLine($"Final result: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in GetTicketTypesAndPrices:");
                Console.WriteLine($"  Message: {ex.Message}");
                Console.WriteLine($"  StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  InnerException: {ex.InnerException.Message}");
                }
                return null;
            }
        }
        // Lấy danh sách suất chiếu theo movieId, branchId và ngày
        public dynamic GetShowTimesByMovieAndDate(string movieId, string branchId, DateTime date)
        {
            try
            {
                if (string.IsNullOrEmpty(movieId) || string.IsNullOrEmpty(branchId))
                {
                    Console.WriteLine("ERROR: MovieId or BranchId is empty");
                    return new List<object>();
                }

                Console.WriteLine($"=== GetShowTimesByMovieAndDate Service Method ===");
                Console.WriteLine($"MovieId: '{movieId}'");
                Console.WriteLine($"BranchId: '{branchId}'");
                Console.WriteLine($"Date: {date:yyyy-MM-dd}");

                var showTimes = _context.ShowTimes
                    .Include(st => st.Room)
                    .Include(st => st.Movie)
                    .Where(st => st.MovieID == movieId &&
                                !st.IsDeleted &&
                                st.Room != null &&
                                !st.Room.IsDeleted &&
                                st.Room.BranchID == branchId &&
                                st.StartTime.Date == date.Date)
                    .OrderBy(st => st.StartTime)
                    .Select(st => new
                    {
                        ShowTimeID = st.ShowTimeID,
                        StartTime = st.StartTime,
                        TimeDisplay = st.StartTime.ToString("HH:mm"),
                        RoomName = st.Room.RoomName,
                        RoomType = st.Room.RoomType,
                        Price = st.Price ?? 0,
                        MovieTitle = st.Movie.Title,
                        MovieDuration = st.Movie.DurationMinutes ?? 120,
                        // Tính số ghế trống
                        TotalSeats = _context.Seats.Count(s => s.RoomID == st.RoomID && !s.IsDeleted),
                        AvailableSeats = _context.Tickets.Count(t => 
                           t.ShowTimeID == st.ShowTimeID && 
                                !t.IsDeleted && 
                                t.Status == "Trống")
                    })
                    .ToList();

                Console.WriteLine($"Query completed. Found {showTimes.Count} showtimes");

                if (showTimes.Count == 0)
                {
                    Console.WriteLine("WARNING: No showtimes found. Checking conditions:");
                    Console.WriteLine($"  - Movie exists: {_context.Movies.Any(m => m.MovieID == movieId && !m.IsDeleted)}");
                    Console.WriteLine($"  - Branch exists: {_context.CinemaBranches.Any(b => b.BranchID == branchId && !b.IsDeleted)}");
                    Console.WriteLine($"  - ShowTimes for movie: {_context.ShowTimes.Count(st => st.MovieID == movieId && !st.IsDeleted)}");
                    Console.WriteLine($"  - ShowTimes for movie & branch: {_context.ShowTimes.Count(st => st.MovieID == movieId && !st.IsDeleted && st.Room.BranchID == branchId)}");
                    Console.WriteLine($"  - ShowTimes for movie & branch & date: {_context.ShowTimes.Count(st => st.MovieID == movieId && !st.IsDeleted && st.Room.BranchID == branchId && st.StartTime.Date == date.Date)}");
                }
                else
                {
                    foreach (var st in showTimes)
                    {
                        Console.WriteLine($"  - {st.ShowTimeID}: {st.StartTime:yyyy-MM-dd HH:mm} in {st.RoomName} ({st.AvailableSeats}/{st.TotalSeats} seats)");
                    }
                }

                return showTimes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in GetShowTimesByMovieAndDate:");
                Console.WriteLine($"  Message: {ex.Message}");
                Console.WriteLine($"  StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  InnerException: {ex.InnerException.Message}");
                }
                return new List<object>();
            }
        }
    }
}
