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
using FrontEnd.Interfaces;

namespace MasjidTracker.FrontEnd.Controllers
{

    [Route("/")]
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly ICacheableService _cacheableService;
        private readonly IUserService userService;
        private readonly string _targetResource;

        private string returnUrl = "?ReturnUrl=%2FEvents%2FIndex";

        public HomeController(ILogger<HomeController> logger, IConfiguration config, ICacheableService cacheableService, IUserService userService)
        {
            _logger = logger;
            _config = config;
            _cacheableService = cacheableService;
            this.userService = userService;
            _targetResource = config["TargetAPIAzureADAPP"];

        }


        [HttpGet]
        public IActionResult Index()
        {
            string path = HttpContext.Request.QueryString.ToString();

            if (path.Contains(returnUrl))
            {
                ViewBag.Redirected = true;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Visitor visitor)
        {
            if (visitor.FirstName == null)
            {
                return RedirectToAction("Index");
            }
            else if (visitor.QrCode == null)
            {
                visitor.PhoneNumber = $"+1{visitor.PhoneNumber}";
                var visitorGuid = await userService.RegisterUser(_config["REGISTER_API_URL"], visitor, _targetResource);

                if (visitorGuid != null)
                {
                    visitor.Id = visitorGuid;
                    visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
                    RegisterCookies(visitor.Id.ToString());

                    if (!visitor.isVerified)
                    {
                        RequestCodeIfProd(visitor);
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
        public ViewResult Signup(Visitor visitorSearch)
        {

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Signup", "Post", path);
            helper.DebugLogger.LogInvocation();
            return View("Registration", visitorSearch);
        }

        [HttpPost]
        public async Task<IActionResult> Signin(Visitor visitorSearch, string redirect)
        {
            if (redirect == "True")
            {
                ViewBag.Redirected = true;
            }

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Signin", "Get", path);
            if (visitorSearch.FirstName != null && visitorSearch.PhoneNumber != null)
            {
                if (!visitorSearch.PhoneNumber.StartsWith("+1"))
                {
                    visitorSearch.PhoneNumber = $"+1{visitorSearch.PhoneNumber}";
                }

                helper.DebugLogger.LogInvocation();
                var url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={visitorSearch.FirstName}&LastName={visitorSearch.LastName}&PhoneNumber={HttpUtility.UrlEncode(visitorSearch.PhoneNumber)}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                var visitor = await userService.GetUsers(url, _targetResource);

                if (null != visitor)
                {
                    await getTitle();
                    await getPrintTitle();

                    RequestCodeIfProd(visitor);

                    RegisterCookies(visitor.Id.ToString());

                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" || !visitor.isVerified)
                    {
                        return View("Partial/VerifyVisitor", visitor);
                    }
                    else
                    {
                        if (redirect == "True")
                        {
                            ViewBag.Redirected = true;
                            ViewBag.VerifiedSoRedirect = true;
                        }
                        return View("Index", visitor);
                    }
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
        [HttpGet]
        [Route("/Signout")]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return View("Index");
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

            await userService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource);
            return View("Index", visitor);
        }

        public async Task<IActionResult> VerifyCode(Visitor visitor, string redirect)
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

                    var resultInfo = await userService.VerifyCode(_config["VERIFY_CODE_API_URL"], smsRequestModel, _targetResource);

                    if (resultInfo != null && resultInfo.VerificationStatus.ToUpper() == "APPROVED" && resultInfo.Id != null)
                    {
                        var url = $"{_config["RETRIEVE_USER_API_URL"]}/{visitor.Id}";
                        visitor = await userService.GetUser(url, _targetResource);
                        if (visitor != null)
                        {
                            visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
                            visitor.isVerified = true;

                            if (redirect == "True")
                            {
                                ViewBag.Redirected = true;
                                ViewBag.VerifiedSoRedirect = true;
                            }
                        }
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

            //this gets the title of the page from respective db depending on the current host url
            await getTitle();
            await getPrintTitle();

            return View("Index", visitor);
        }

        [HttpPost]
        public async void RegisterCookies(string VisitorId)
        {

            var v_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (v_id == null)
            {
                var claims = new List<Claim>{
                    new Claim(ClaimTypes.NameIdentifier, VisitorId),
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
            }

        }

        public async Task<string> getTitle()
        {
            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "getTitle", "Get", path);
            string cururl = HttpContext.Request.Host.ToString();
            Common.Models.Setting mysetting = new Common.Models.Setting(cururl, "OnlinePassTitle");
            string url = $"{_config["RETRIEVE_SETTINGS"]}?domain={mysetting.domain}&key={mysetting.key}";

            string title = await _cacheableService.GetSetting(url, mysetting.domain, mysetting.key, _targetResource, mysetting);
            ViewBag.pageTitle = title;
            return title;

        }

        public async Task<string> getPrintTitle()
        {
            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "getPrintTitle", "Get", path);
            string cururl = HttpContext.Request.Host.ToString();
            Common.Models.Setting mysetting = new Common.Models.Setting(cururl, "PrintPassTitle");
            string url = $"{_config["RETRIEVE_SETTINGS"]}?domain={mysetting.domain}&key={mysetting.key}";
            string title = await _cacheableService.GetSetting(url, mysetting.domain, mysetting.key, _targetResource, mysetting);

            ViewBag.printTitle = title;
            return title;
        }

        public IActionResult Error(string returnUrl, int? statusCode = null)
        {

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
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
            return View("CustomError");
        }

        private async void RequestCodeIfProd(Visitor visitor){
            
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var smsRequestModel = new SMSRequestModel()
                {
                    Id = visitor.Id.ToString(),
                    PhoneNumber = visitor.PhoneNumber
                };
                await userService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource);
            }
            else
            {
                //copied this code from VerifyCode function to simulate verification in nonProd environment
                visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(visitor.Id.ToString());
                visitor.isVerified = true;
            }
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

    }
}
