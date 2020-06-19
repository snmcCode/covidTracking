using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasjidTracker.FrontEnd.Models;
using FrontEnd;
using System.Threading.Tasks;

namespace MasjidTracker.FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
 
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Landing()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult Index()
        {         
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Signin(string id = null)
        //{
        //    Visitor visitor = null;
        //    if(id != null)
        //    {
        //        visitor = await UserService.GetUser(id);
        //        visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
        //    }

        //    return View("Index", visitor);
        //}

        [HttpPost]
        public async Task<IActionResult> Signin(Visitor visitor)
        {        
            if (visitor.FirstName != null)
            {
                visitor = await UserService.GetUsers(visitor);
                visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
            }

            return View("Index", visitor);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Visitor visitor)
        {
            if (visitor.FirstName == null)
            {
                return RedirectToAction("Landing");
            }
            else if (visitor.QrCode == null)
            {
                var visitorGuid = await UserService.RegisterUser(visitor);

                if(visitorGuid != null)
                {
                    visitor.Id = visitorGuid;
                    visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
                }
                else
                {
                    _logger.LogError("Failed creating user");
                }
                
            }

            return View(visitor);
        }

        [HttpGet("Registration")]
        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult Signout()
        {
            return RedirectToAction("Index");
        }

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

    }
}
