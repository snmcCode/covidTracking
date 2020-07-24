using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasjidTracker.FrontEnd.Models;
using FrontEnd;
using System.Threading.Tasks;
using FrontEnd.Models;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace MasjidTracker.FrontEnd.Controllers
{
    [Route("/")]
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];

        }
        
        [HttpGet]
        public IActionResult Index()
        {            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signin(Visitor visitorSearch)
        {
            if (visitorSearch.FirstName != null)
            {
                if(!visitorSearch.PhoneNumber.StartsWith("+1"))
                {
                    visitorSearch.PhoneNumber = $"+1{visitorSearch.PhoneNumber}";
                }


                var url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={visitorSearch.FirstName}&LastName={visitorSearch.LastName}&PhoneNumber={HttpUtility.UrlEncode(visitorSearch.PhoneNumber)}";
                var visitor = await UserService.GetUsers(url, _targetResource);

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

                        await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource);
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
                var visitorGuid = await UserService.RegisterUser(_config["REGISTER_API_URL"], visitor, _targetResource);

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

                await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource);
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

            await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource);
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

            var resultInfo = await UserService.VerifyCode(_config["VERIFY_CODE_API_URL"], smsRequestModel, _targetResource);

            if(resultInfo != null && resultInfo.VerificationStatus.ToUpper() == "APPROVED" && resultInfo.Id != null)
            {
                var url = $"{_config["RETRIEVE_USER_API_URL"]}/{visitor.Id}";
                visitor = await UserService.GetUser(url, _targetResource);                
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
