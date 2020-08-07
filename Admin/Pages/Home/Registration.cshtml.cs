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
using Microsoft.CodeAnalysis;

namespace Admin.Pages.Home
{
    public class RegistrationModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        [BindProperty]
        public VisitorModel Visitor { get; set; }

        [BindProperty]
        public bool BypassVerification { get; set; }

        [BindProperty]
        public bool VerifyLater { get; set; }

        public bool AgreeCheckbox { get; set; }

        public IActionResult OnPost()
        {

            return Page();
        }
    }
}