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
        public async Task<IActionResult> Signin(Visitor visitorSearch)
        {
            if (visitorSearch.FirstName != null)
            {
                var visitor = await UserService.GetUsers(visitorSearch);

                if(visitor != null)
                {
                    visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
                }
                else
                {
                    ViewBag.SigninFailed = true;
                }

                return View("Index", visitor);
            }
            else
            {
                ViewBag.SigninFailed = true;
            }

            return View("Index");
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
                visitor.PhoneNumber = $"+1{visitor.PhoneNumber}";
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

            ViewBag.Organization = visitor.RegistrationOrg;
            return View(visitor);
        }

        [HttpGet("Registration/{organization?}")]
        public IActionResult Registration(string organization = "Online")
        {
            ViewBag.Organization = organization;
            return View();
        }

        public IActionResult Signout(Visitor visitor)
        {
            if (visitor.RegistrationOrg != Organization.Online)
            {
                return RedirectToAction(visitor.RegistrationOrg.ToString(), "Registration");
            }

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
