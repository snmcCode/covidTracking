using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasjidTracker.FrontEnd.Models;
using FrontEnd;
using System.Threading.Tasks;
using FrontEnd.Models;
using Microsoft.Extensions.Configuration;
using System.Web;
using Common.Utilities;
using System.Runtime.InteropServices;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
namespace MasjidTracker.FrontEnd.Controllers
{

    [Route("/")]
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        private string returnUrl {get; set;}

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

        public async Task<string> getTitle()
        {
            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "getTitle", "Get", path);
            string cururl = HttpContext.Request.Host.ToString();
            Common.Models.Setting mysetting = new Common.Models.Setting(cururl, "OnlinePassTitle");
            string url = $"{_config["RETRIEVE_SETTINGS"]}?domain={mysetting.domain}&key={mysetting.key}";
            string title = await UserService.getSetting(url, _targetResource, mysetting, _logger);
            ViewBag.pageTitle = title;
            return title;

        }

        public async Task<string> getPrintTitle()
        {
            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "getPrintTitle", "Get", path);
            string cururl = HttpContext.Request.Host.ToString();
            Common.Models.Setting mysetting = new Common.Models.Setting(cururl, "PrintPassTitle");
            string url = $"{_config["RETRIEVE_SETTINGS"]}?domain={mysetting.domain}&key={mysetting.key}";
            string title = await UserService.getSetting(url, _targetResource, mysetting, _logger);
            ViewBag.printTitle = title;
            return title;
        }

        [HttpPost]
        public ViewResult Signup(Visitor visitorSearch)
        {

            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "Signup", "Post", path);
            helper.DebugLogger.LogInvocation();
            return View("Registration", visitorSearch);
        }

        [HttpPost]
        //[Route("[type]")]
        public async Task<IActionResult> Signin(Visitor visitorSearch, string type)
        {
            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "Signin", "Get", path);
            if (visitorSearch.FirstName != null && visitorSearch.PhoneNumber != null)
            {
                if (!visitorSearch.PhoneNumber.StartsWith("+1"))
                {
                    visitorSearch.PhoneNumber = $"+1{visitorSearch.PhoneNumber}";
                }

                helper.DebugLogger.LogInvocation();
                var url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={visitorSearch.FirstName}&LastName={visitorSearch.LastName}&PhoneNumber={HttpUtility.UrlEncode(visitorSearch.PhoneNumber)}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                var visitor = await UserService.GetUsers(url, _targetResource, _logger);

                if (null != visitor)
                {
                    await getTitle();
                    await getPrintTitle();
                    visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());

                    var smsRequestModel = new SMSRequestModel()
                    {
                        Id = visitor.Id.ToString(),
                        PhoneNumber = visitor.PhoneNumber
                    };

                    await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource, _logger);

                    var claims = new List<Claim>{
                            new Claim(ClaimTypes.NameIdentifier, visitor.Id.ToString()),
                        };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    { };

                    await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);
                    ViewBag.CookiesSet = true;
                    return View("Index", visitor);
                }

                else
                {
                    ViewBag.SigninFailed = true;
                }
            }
            else
            {
                ViewBag.SigninFailed = true;
            }

            return View("Index");
        }


        [HttpPost]
        public async Task<IActionResult> RegisterCookies(string VisitorId){
            ViewBag.CookiesSet = true;
            var claims = new List<Claim>{
                    new Claim(ClaimTypes.NameIdentifier, VisitorId),
                };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            { };

            await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
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
                var visitorGuid = await UserService.RegisterUser(_config["REGISTER_API_URL"], visitor, _targetResource, _logger);

                if (visitorGuid != null)
                {
                    visitor.Id = visitorGuid;
                    visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());

                    if (!visitor.isVerified)
                    {
                        var smsRequestModel = new SMSRequestModel()
                        {
                            Id = visitor.Id.ToString(),
                            PhoneNumber = visitor.PhoneNumber
                        };

                        await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource, _logger);
                    }

                    ViewBag.Organization = visitor.RegistrationOrg;
                    return View(visitor);

                }
                else
                {
                    _logger.LogError("Failed creating user");
                    ErrorViewModel error = new ErrorViewModel();
                    return View(error);
                }

            }
            ViewBag.Organization = visitor.RegistrationOrg;
            return View(visitor);

        }

        [HttpPost]
        [HttpGet]
        [Route("/Signout")]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            ViewBag.CookiesSet = false;
            return View("Index");
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

        [Route("covid")]
        public IActionResult Covid()
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

            await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource, _logger);
            return View("Index", visitor);
        }

        public async Task<IActionResult> VerifyCode(Visitor visitor)
        {

            string strcode = visitor.VerificationCode;
            if (strcode != null)
            {

                strcode = strcode.ToString().Trim();
                if (strcode.Length == 4 && strcode != "")
                {
                    var smsRequestModel = new SMSRequestModel()
                    {
                        Id = visitor.Id.ToString(),
                        PhoneNumber = visitor.PhoneNumber,
                        VerificationCode = visitor.VerificationCode
                    };

                    var resultInfo = await UserService.VerifyCode(_config["VERIFY_CODE_API_URL"], smsRequestModel, _targetResource, _logger);


                    if (resultInfo != null && resultInfo.VerificationStatus.ToUpper() == "APPROVED" && resultInfo.Id != null)
                    {
                        var url = $"{_config["RETRIEVE_USER_API_URL"]}/{visitor.Id}";
                        visitor = await UserService.GetUser(url, _targetResource, _logger);
                    }
                    else
                    {
                        ViewBag.RequestSuccess = "False";
                        ViewBag.RequestMessage = "The code you entered is incorrect";
                    }
                }
                else
                {
                    ViewBag.RequestSuccess = "False";
                    ViewBag.RequestMessage = "Please make sure The 4-digit code is in the correct format";
                }

            }
            else
            {
                ViewBag.RequestSuccess = "False";
                ViewBag.RequestMessage = "Don't forget to enter your complete varification code";
            }
            if (visitor != null)
            {
                visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
            }
            //this gets the title of the page from respective db depending on the current host url
            await getTitle();
            await getPrintTitle();

            return View("Index", visitor);
        }

        [Route("/Account/Login")]
        public IActionResult Error(string returnUrl, int? statusCode = null)
        {

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)){
                return View(401.ToString());    
            }
            if (statusCode.HasValue)
            {
                if (statusCode.Value == 404)
                {
                    var viewName = statusCode.ToString();
                    return View(viewName);
                }
            }
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

    }
}
