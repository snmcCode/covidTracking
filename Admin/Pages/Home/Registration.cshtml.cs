﻿using System;
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

        [BindProperty]
        [Required(ErrorMessage = "Visitor must agree to the rules and privacy policy")]
        public bool AgreeCheckbox { get; set; }

        [BindProperty(SupportsGet = true)]
        public OrganizationModel Organization { get; set; }

        public async Task<IActionResult> OnPost()
        {


            Visitor.IsVerified = VerifyLater == "considerVerified";

            Visitor.RegistrationOrg = HttpContext.User.FindFirstValue(ClaimTypes.Name);

            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "RegisterVisitor", "Post", path);
            if (Visitor.FirstName != null)
            {
                if (Visitor.Email == null)
                {
                    Visitor.Email = "";
                }

                helper.DebugLogger.LogCustomError("Email is:" + Visitor.Email.GetType());
                helper.DebugLogger.LogInvocation();

                var url = $"{_config["REGISTER_API_URL"]}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));

                Visitor.PhoneNumber = $"+1{Visitor.PhoneNumber}";

                var bodyData =
                    new
                    {
                        FirstName = Visitor.FirstName,
                        LastName = Visitor.LastName,
                        Email = Visitor.Email,
                        PhoneNumber = Visitor.PhoneNumber,
                        IsMale = Visitor.IsMale,
                        IsVerified = Visitor.IsVerified
                    };
                string jsonBody = JsonConvert.SerializeObject(bodyData);

                var visitorGuid = await UserService.RegisterUser(url, _targetResource, _logger, jsonBody);

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
                        return RedirectToPage("VerifyVisitor", new {Visitor.Id, Visitor.PhoneNumber, Visitor.FirstName, Visitor.LastName});
                    }
                    return RedirectToPage("/Home/View", new {Visitor.Id, Visitor.FirstName, Visitor.LastName});
                }
            }

            return Page();

        }


    }
}