using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Filters;
using Web.Service;

namespace Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly ICinemaBranchService _cinemaBranchService;
        private readonly IMovieService_Cus _movieService_Cus;
        private readonly IShowTimeService _showTimeService; // THÊM


        public MovieController(
            ICinemaBranchService cinemaBranchService,
            IMovieService_Cus movieService_Cus,
            IShowTimeService showTimeService) // THÊM
        {
            _cinemaBranchService = cinemaBranchService;
            _movieService_Cus = movieService_Cus;
            _showTimeService = showTimeService; // THÊM
        }

        [LoadCinemaBranches]
        public IActionResult Index()
        {
            // Lấy danh sách các thành phố có rạp
            var cities = _cinemaBranchService.GetListCityBranches();
            ViewData["lstCity"] = cities;
            // Lấy danh sách phim đang chiếu và sắp chiếu
            var nowShowing = _movieService_Cus.GetNowShowingMoviesAsync(12).Result;
            var comingSoon = _movieService_Cus.GetComingSoonMoviesAsync(12).Result;
            // Truyền dữ liệu qua ViewData
            ViewData["NowShowing"] = nowShowing;
            ViewData["ComingSoon"] = comingSoon;
            return View();
        }

        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var movie = _movieService_Cus.GetMovieByIdAsync(id).Result;

            if (movie == null)
            {
                return NotFound();
            }

            // Lấy danh sách các thành phố có rạp (cho header)
            var cities = _cinemaBranchService.GetListCityBranches();
            ViewData["lstCity"] = cities;

            // Lấy thành phố mặc định (thành phố đầu tiên)
            var defaultCity = cities.FirstOrDefault() ?? "HỒ CHÍ MINH";

            // Lấy danh sách rạp chiếu phim này ở thành phố mặc định
            var branches = _cinemaBranchService.GetBranchesByCityAndMovie(defaultCity, id);
            ViewData["Branches"] = branches;
            ViewData["SelectedCity"] = defaultCity;

            // Lấy lịch chiếu cho rạp đầu tiên (nếu có)
            //if (branches.Any())
            //{
            //    var firstBranch = branches.First();
            //    var today = DateTime.Today;
            //    var showTimes = _showTimeService.GetShowTimesByBranchMovieDate(
            //        firstBranch.BranchID,
            //        id,
            //        today
            //    );
            //    ViewData["ShowTimes"] = showTimes;
            //    ViewData["SelectedBranch"] = firstBranch;
            //}

            return View(movie);
        }

        // API: Lấy danh sách rạp theo thành phố và phim
        [HttpGet]
        public IActionResult GetBranchesByCity(string city, string movieId)
        {
            var branches = _cinemaBranchService.GetBranchesByCityAndMovie(city, movieId);
            return Json(branches.Select(b => new
            {
                branchId = b.BranchID,
                branchName = b.BranchName,
                address = b.Address,
                district = b.District
            }));
        }

        // API: Lấy lịch chiếu theo rạp, phim và ngày
        [HttpGet]
        public IActionResult GetShowTimes(string branchId, string movieId, string date)
        {
            if (!DateTime.TryParse(date, out var selectedDate))
            {
                return BadRequest("Invalid date format");
            }

            var showTimes = _showTimeService.GetShowTimesByBranchMovieDate(branchId, movieId, selectedDate);
            return Json(showTimes);
        }


        // API: Lấy thông tin giá vé theo suất chiếu
        [HttpGet]
        public IActionResult GetTicketPrices(string showTimeId)
        {
            var ticketPrices = _showTimeService.GetTicketPricesByShowTime(showTimeId);
            return Json(ticketPrices);
        }

        // Render ra các ghế 
        public async Task<IActionResult> GetSeatingLayout(string showTimeId, Guid currentCustomerId)
        {
            var seats = await _movieService_Cus.GetSeatingLayoutAsync(showTimeId, currentCustomerId);
            return Json(seats);
        }

        // Xử lý click đặt ghế
        [HttpPost]
        public async Task<IActionResult> SelectSeats(string showTimeId, string seatId)
        {
            var customerId = User.FindFirstValue("CustomerID");
            Console.WriteLine(" ------------ " + showTimeId + " " + seatId + " " + customerId);
            var success = await _movieService_Cus.TrySelectSeatAsync(showTimeId, seatId, Guid.Parse(customerId));
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> DeselectSeat(string showTimeId, string seatId)
        {
            var customerId = User.FindFirstValue("CustomerID");
            var success = await _movieService_Cus.DeselectSeatAsync(showTimeId, seatId, Guid.Parse(customerId));
            return Json(new { success });
        }

    }
}