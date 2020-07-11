using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasjidTracker.FrontEnd.Models;
using FrontEnd;
using System.Threading.Tasks;
using FrontEnd.Models;

namespace MasjidTracker.FrontEnd.Controllers
{
    [Route("/")]
    [Route("[controller]/[action]")]
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
                if(!visitorSearch.PhoneNumber.StartsWith("+1"))
                {
                    visitorSearch.PhoneNumber = $"+1{visitorSearch.PhoneNumber}";
                }

                var visitor = await UserService.GetUsers(visitorSearch);

                if(visitor != null)
                {
                    visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
                    if (!visitor.isVerified)
                    {
                        var smsRequestModel = new SMSRequestModel()
                        {
                            Id = visitor.Id.ToString(),
                            PhoneNumber = visitor.PhoneNumber
                        };

                        await UserService.RequestCode(smsRequestModel);
                    }
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

            if(!visitor.isVerified)
            {
                var smsRequestModel = new SMSRequestModel()
                {
                    Id = visitor.Id.ToString(),
                    PhoneNumber = visitor.PhoneNumber
                };

                await UserService.RequestCode(smsRequestModel);
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

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Rules()
        {
            return View();
        }

        public IActionResult Instructions()
        {
            return View();
        }


        public IActionResult Contact()
        {
            return View();
        }

        public async Task<IActionResult> RequestCode(Visitor visitor)
        {
            var smsRequestModel = new SMSRequestModel()
            {
                Id = visitor.Id.ToString(),
                PhoneNumber = visitor.PhoneNumber
            };

            if (visitor != null)
            {
                visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
            }

            ViewBag.RequestSuccess = "True";
            ViewBag.RequestMessage = "Verification code sent";
            ViewBag.DisableRequestButton = true;

            await UserService.RequestCode(smsRequestModel);
            return View("Index", visitor);
        }

        public async Task<IActionResult> VerifyCode(Visitor visitor)
        {
            var smsRequestModel = new SMSRequestModel()
            {
                Id = visitor.Id.ToString(),
                PhoneNumber = visitor.PhoneNumber,
                VerificationCode = visitor.VerificationCode
            };

            var resultInfo = await UserService.VerifyCode(smsRequestModel);

            if(resultInfo != null && resultInfo.VerificationStatus.ToUpper() == "APPROVED" && resultInfo.Id != null)
            {
                visitor = await UserService.GetUser(resultInfo.Id.ToString());                
            }
            else
            {
                ViewBag.RequestSuccess = "False";
                ViewBag.RequestMessage = "The code you entered is incorrect";
            }

            if (visitor != null)
            {
                visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
            }

            return View("Index", visitor);
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

    }
}
