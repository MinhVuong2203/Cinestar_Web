using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Filters;
using Web.Models;
using Web.Service;

namespace Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly ICinemaBranchService _cinemaBranchService;
        private readonly IMovieService_Cus _movieService_Cus;
        private readonly IShowTimeService _showTimeService;
        private readonly IProductService _productService;


        public MovieController(
            ICinemaBranchService cinemaBranchService,
            IMovieService_Cus movieService_Cus,
            IShowTimeService showTimeService,
            IProductService productService)
        {
            _cinemaBranchService = cinemaBranchService;
            _movieService_Cus = movieService_Cus;
            _showTimeService = showTimeService;
            _productService = productService;
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

        public async Task<IActionResult> Details(string id)
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

            // ===== KIỂM TRA PHIM ĐANG CHIẾU HAY SẮP CHIẾU =====
            var now = DateTime.Now;
            bool isComingSoon = movie.StartTime.HasValue && movie.StartTime.Value > now;

            ViewData["IsComingSoon"] = isComingSoon;

            // Nếu là phim đang chiếu, load đầy đủ thông tin đặt vé
            if (!isComingSoon)
            {
                // Lấy thành phố mặc định
                var defaultCity = cities.FirstOrDefault();

                // Lấy danh sách rạp chiếu phim này ở thành phố mặc định
                var branches = _cinemaBranchService.GetBranchesByCityAndMovie(defaultCity, id);
                ViewData["Branches"] = branches;
                ViewData["SelectedCity"] = defaultCity;

                // ✅ THÊM: Lấy ngày hiện tại để load showtimes
                ViewData["DefaultDate"] = DateTime.Today.ToString("yyyy-MM-dd");

                // ===== LẤY DANH SÁCH SẢN PHẨM =====
                try
                {
                    Console.WriteLine("\n🎬 [CONTROLLER] ========== PRODUCT LOADING START ==========");

                    if (_productService == null)
                    {
                        Console.WriteLine("❌ [CONTROLLER] _productService is NULL - Dependency Injection failed!");
                        ViewData["Products"] = new Dictionary<string, List<Product>>();
                        return View(movie);
                    }

                    Console.WriteLine("✅ [CONTROLLER] _productService is injected successfully");
                    Console.WriteLine($"[CONTROLLER] Service type: {_productService.GetType().Name}");

                    var products = await _productService.GetAllProductsGroupedByTypeAsync();

                    Console.WriteLine($"\n[CONTROLLER] Received: {products?.Count ?? 0} categories");

                    if (products != null && products.Any())
                    {
                        foreach (var category in products)
                        {
                            Console.WriteLine($"[CONTROLLER]   ✓ {category.Key}: {category.Value.Count} products");
                        }
                        ViewData["Products"] = products;
                    }
                    else
                    {
                        Console.WriteLine("⚠️ [CONTROLLER] No products returned from service");
                        ViewData["Products"] = new Dictionary<string, List<Product>>();
                    }

                    Console.WriteLine("🎬 [CONTROLLER] ========== PRODUCT LOADING END ==========\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [CONTROLLER] EXCEPTION: {ex.Message}");
                    Console.WriteLine($"[CONTROLLER] Type: {ex.GetType().Name}");
                    Console.WriteLine($"[CONTROLLER] Stack Trace:\n{ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[CONTROLLER] Inner Exception: {ex.InnerException.Message}");
                    }

                    ViewData["Products"] = new Dictionary<string, List<Product>>();
                }
            }
            else
            {
                // Phim sắp chiếu - không load thông tin đặt vé
                ViewData["Branches"] = new List<CinemaBranch>();
                ViewData["SelectedCity"] = "";
                ViewData["Products"] = new Dictionary<string, List<Product>>();
            }

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

        //lấy vé theo ghế ID, showtime ID
        [HttpGet]
        public async Task<IActionResult> GetTicketBySeatId(string showTimeId, string seatId)
        {
            var ticket = await _movieService_Cus.GetTicketBySeatIdAsync(showTimeId, seatId);
            return Json(ticket);

        }
        public IActionResult showing()
        {
            var cities = _cinemaBranchService.GetListCityBranches();
            ViewData["lstCity"] = cities;

           
            var nowShowingMovies = _movieService_Cus.GetNowShowingMoviesAsync(100).Result;

            return View(nowShowingMovies);
        }
        public IActionResult upcoming()
        {
            var cities = _cinemaBranchService.GetListCityBranches();
            ViewData["lstCity"] = cities;

          
            var comingSoonMovies = _movieService_Cus.GetComingSoonMoviesAsync(100).Result;

            return View(comingSoonMovies);
        }
    }
}