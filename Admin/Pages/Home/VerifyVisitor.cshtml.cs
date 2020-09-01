using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Admin.Services;
using Admin.Util;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Admin.Pages.Home
{
    public class VerifyVisitorModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public VisitorModel Visitor { get; set; }

        [BindProperty]
        public string VerificationCode { get; set; }

        private readonly ILogger<VisitorModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        public VerifyVisitorModel(ILogger<VisitorModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        public async void OnGet(VisitorModel visitor)
        {
            Visitor = visitor;
            Visitor.PhoneNumber = $"+1{Visitor.PhoneNumber}";
            Visitor.IsVerified = false;

            await OnPostRequestCode();
        }

        // Verify SMS Verification Code once it's entered
        public async Task<IActionResult> OnPostVerifyCode()
        {
            var smsRequestModel = new SMSRequestModel()
            {
                Id = Visitor.Id.ToString(),
                PhoneNumber = Visitor.PhoneNumber,
                VerificationCode = Visitor.VerificationCode
            };

            var resultInfo = await UserService.VerifyCode(_config["VERIFY_CODE_API_URL"], smsRequestModel, _targetResource, _logger);

            if (resultInfo != null && resultInfo.VerificationStatus.ToUpper() == "APPROVED" && resultInfo.Id != null)
            {
                return RedirectToPage("../Home/View", new { Visitor.Id, Visitor.FirstName, Visitor.LastName });
            }
            else
            {
                ViewData["RequestMessage"] = "The code you entered is incorrect";
            }

            return Page();
        }

        // Send SMS Verification Code
        public async Task<IActionResult> OnPostRequestCode()
        {
            var smsRequestModel = new SMSRequestModel()
            {
                Id = Visitor.Id.ToString(),
                PhoneNumber = Visitor.PhoneNumber
            };

            Visitor.VerificationCode = await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource, _logger);
            return Page();
        }

        // Cancel Verification and just go to view, will be considered a 'verify later'
        public IActionResult OnPostCancel()
        {
            return RedirectToPage("/Home/View", new { Visitor.Id, Visitor.FirstName, Visitor.LastName });
        }

    }
}