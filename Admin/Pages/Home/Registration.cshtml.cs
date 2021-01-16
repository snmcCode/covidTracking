using System;
using System.Web;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Common.Models;
using Common.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Admin.Models;
using Admin.Services;
using Admin.Util;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace Admin.Pages.Home
{
    public class RegistrationModel : PageModel
    {
        private readonly ILogger<RegistrationModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;


        public RegistrationModel(ILogger<RegistrationModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        [BindProperty]
        public VisitorModel Visitor { get; set; }

        [BindProperty]
        [Required]
        [DisplayName("Bypass Verification")]
        public bool BypassVerification { get; set; }

        [BindProperty]
        public string VerifyLater { get; set; }

        public bool isTrue => true; // using this is hack-y but it works

        [BindProperty]
        [Required]
        [Compare(nameof(isTrue), ErrorMessage = "Visitor must agree to the rules and privacy policy")]
        public bool AgreeCheckbox { get; set; }

        [BindProperty(SupportsGet = true)]
        public OrganizationModel Organization { get; set; }

        public async Task<string> getPrintTitle()
        {
            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "getPrintTitle", "Get", path);
            string cururl = HttpContext.Request.Host.ToString();
            Common.Models.Setting mysetting = new Common.Models.Setting(cururl, "PrintPassTitle");
            string url = $"{_config["RETRIEVE_SETTINGS"]}?domain={mysetting.domain}&key={mysetting.key}";
            string title = await UserService.getSetting(url, _targetResource, mysetting, _logger);
            return title;

        }

        public IActionResult OnGet(OrganizationModel organization)
        {
            if (!verifyParams())
            {
                return RedirectToPage("/Errors/404");
            }
            return Page();
        }

        // Return true if params are valid / correct for current session
        private bool verifyParams()
        {
            return Organization.Id == Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)) &&
               Organization.Name == HttpContext.User.FindFirstValue(ClaimTypes.Name);
        }

        // When 'Register' is clicked
        public async Task<IActionResult> OnPost()
        {

            Visitor.IsVerified = VerifyLater == "considerVerified";

            Visitor.RegistrationOrg = HttpContext.User.FindFirstValue(ClaimTypes.Name);

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "RegisterVisitor", "Post", path);
            if (Visitor.FirstName != null)
            {

                var url = $"{_config["REGISTER_API_URL"]}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));

                var temp_phone = $"+1{Visitor.PhoneNumber}";

                var bodyData =
                    new
                    {
                        FirstName = Visitor.FirstName,
                        LastName = Visitor.LastName,
                        Email = Visitor.Email,
                        PhoneNumber = temp_phone,
                        IsMale = Visitor.IsMale,
                        IsVerified = Visitor.IsVerified,
                        RegistrationOrg = Organization.Id
                    };
                string jsonBody = JsonConvert.SerializeObject(bodyData);

                var visitorGuid = await UserService.RegisterUser(url, _targetResource, _logger, jsonBody);

                var visitorTitle = await getPrintTitle();

                if (visitorGuid == null)
                {
                    helper.DebugLogger.LogCustomError("Visitor Guid is null");
                }
                else
                {
                    Visitor.Id = visitorGuid;

                    Visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(Visitor.Id.ToString());

                    if (!BypassVerification)
                    {
                        return RedirectToPage("/Home/VerifyVisitor", new { Visitor.Id, Visitor.PhoneNumber, Visitor.FirstName, Visitor.LastName });
                    }
                    return RedirectToPage("/Home/View", new { Visitor.Id, Visitor.FirstName, Visitor.LastName, printTitle = visitorTitle });
                }
            }

            return Page();
        }
    }
}