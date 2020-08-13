using System;
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

        public string Org { get; set; }

        [BindProperty]
        public VisitorModel Visitor { get; set; }

        [BindProperty]
        [Required]
        public bool BypassVerification { get; set; }

        [BindProperty]
        public string VerifyLater { get; set; }

        public bool AgreeCheckbox { get; set; }

        [BindProperty(SupportsGet = true)]
        public OrganizationModel Organization { get; set; }

        public IActionResult OnGet(OrganizationModel organization)
        {
            if (organization == null || organization.Name == null)
            {
                return RedirectToPage("../Index");
            }
            else
            {
                Organization = organization;
                Org = Organization.Name;
                return Page();
            }
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Visitor.IsVerified = VerifyLater == "VerifyNow";
            Visitor.RegistrationOrg = Organization.Name;
            
            if (Visitor.FirstName == null || !BypassVerification)
            {
                return Page();
            } else if (Visitor.QrCode == null)
            {
                Visitor.PhoneNumber = $"+1{Visitor.PhoneNumber}";
                var VisitorGuid = await UserService.RegisterUser(_config["REGISTER_API_URL"], Visitor, _targetResource, _logger);

                if (VisitorGuid != null)
                {
                    Visitor.Id = VisitorGuid;
                    Visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(Visitor.Id.ToString());

                    if (!Visitor.IsVerified && !BypassVerification)
                    {
                        // Needs to be verified now
                        return Page();
                    }
                    else
                    {
                        // Already verified or will be verified later
                        return RedirectToPage("/Home/View", Visitor);
                    }
                    /*
                    if (!Visitor.IsVerified && !BypassVerification)
                    {
                        var smsRequestModel = new SMSRequestModel()
                        {
                            Id = Visitor.Id.ToString(),
                            PhoneNumber = Visitor.PhoneNumber
                        };

                        await UserService.RequestCode(_config["REQUEST_CODE_API_URL"], smsRequestModel, _targetResource, _logger);

                    }
                }
                else
                {
                    _logger.LogError("Failed creating user");
                    return RedirectToPage("../Error");
                }*/
                }
            }
            return RedirectToPage("/Home/VerifyVisitor", Visitor);

        }

    }
}