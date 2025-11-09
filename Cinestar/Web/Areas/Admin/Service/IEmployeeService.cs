using Web.Models;

namespace Web.Areas.Admin.Service
{
    public interface IEmployeeService
    {
        Task<bool> IsPhoneExist(string phone, Guid? excludeEmployeeId = null);
        Task<bool> IsEmailExist(string email, Guid? excludeEmployeeId = null);
        Task<bool> IsCCCDExist(string cccd, Guid? excludeEmployeeId = null);
        Task<bool> IsUsernamexist(string username, Guid? excludeEmployeeId = null);
        bool CheckBirthDate(DateOnly? BirthDate);
        public Task<IEnumerable<Employee>> GetAllEmployees();
        public Task<Employee?> GetEmployeeById(Guid id);     
        public Task<bool> UpdateEmployee(Employee employee);
        //public Task<bool> DeleteEmployee(Guid id); // Soft delete
        public Task<IEnumerable<string>> GetAllRoles();
        public Task<bool> CreateEmployee(Employee employee);
        public Task DeteleEmployee(Guid id);
        public double GetSalaryById(Guid id);

        //lấy danh sách phim theo chi nhánh của nhân viên
        public List<Movie> GetMoviesByEmployeeBranchId(string branchId);
        // Lấy thông tin loại vé, số lượng vé và giá của vé phim tại chi nhánh
        public dynamic GetTicketTypesAndPrices(string movieId, string branchId, string showTimeId);

        // Lấy danh sách suất chiếu theo movieId, branchId và ngày
        public dynamic GetShowTimesByMovieAndDate(string movieId, string branchId, DateTime date);

        //lấy tên phòng chiếu theo movieId, branchId, ngày và suất chiếu
        public dynamic GetRoomNameByMovieShowTimeDate(string movieId, string branchId, DateTime date, string showTime);
    }
}
