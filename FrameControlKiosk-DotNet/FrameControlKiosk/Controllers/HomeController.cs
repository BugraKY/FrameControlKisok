using FrameControlKiosk.Data;
using FrameControlKiosk.Models;
using FrameControlKiosk.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FrameControlKiosk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return Redirect("/configuration/main");
            try
            {
                var station = await _context.Station.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(ex.Message);
            }

            return View();
        }
        [HttpPost("/auth")]
        public bool AuthClient([FromBody] Station data)
        {
            Thread.Sleep(100);
            try
            {
                return _context.Station.Any(x => x.IP == data.IP && x.Mac == data.Mac && x.Text == data.Text && x.StationNum == data.StationNum);
            }
            catch (Exception)
            {
                throw;
            }
           
        }
        [HttpGet("stat")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/get-time")]
        public string GetDateTime()
        {
            return DateTime.Now.ToString("dd/MM/yyyy - HH:mm");
        }
        /*
        public static ConnectionStatus IsDatabaseConnected(ApplicationDbContext context)
        {
            try
            {
                context.Database.OpenConnection();
                context.Database.CloseConnection();
                return new ViewModels.ConnectionStatus { Success = true };
            }
            catch (Exception ex)
            {
                return new ViewModels.ConnectionStatus { Success = false, Exception = ex };
            }
        }*/
    }
}