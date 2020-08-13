using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Common.Models;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using Admin.Models;

namespace Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        [BindProperty(SupportsGet = true)]
        [Required]
        public OrganizationModel Organization { get; set; }
        
        public IActionResult OnPost()
        {
            // string path = HttpContext.Request.Path;
            // Helper helper = new Helper(_logger, "Organization Signin", "Get", path);

            if (Organization.LoginName != null && Organization.LoginSecretHash != null)
            {
                // helper.DebugLogger.LogInvocation();
                // var url = $"{_config["RETRIEVE_USERS_API_URL"]}?LoginName={Organization.LoginName}&LastName={Organization.LoginSecretHash}";
                Organization.Name = "SNMC";
                Organization.LoginName = null;
                Organization.LoginSecretHash = null;
                return RedirectToPage("/Home/Home", Organization);
            }

            return Page();
        }
    }
}