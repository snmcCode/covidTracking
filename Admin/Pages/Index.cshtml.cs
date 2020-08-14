using System;
using System.Web;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Common.Models;
using Newtonsoft.Json;
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
        private Boolean SigninFailed {get; set;}

        [BindProperty(SupportsGet = true)]
        [Required]
        public OrganizationModel Organization { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }
        
        // when the sign in button is pressed
        public async Task<IActionResult> OnPost()
        {
            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "Signin", "Post", path);
            if (Organization.LoginName != null)
            {
                helper.DebugLogger.LogInvocation();
                // var url = $"{_config["AUTH_ADMIN_API_URL"]}?Username={Organization.LoginName}&Password={Organization.LoginSecretHash}";

                var url = $"{_config["AUTH_ADMIN_API_URL"]}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));

                var bodyData =
                        new
                            {
                                Username = Organization.LoginName,
                                Password = Organization.LoginSecretHash
                            };
                
                 var data = new { body = new [] {
                                    new {Username = Organization.LoginName,
                                         Password = Organization.LoginSecretHash}
                                }};

                string jsonBody = JsonConvert.SerializeObject(bodyData);
                Console.WriteLine("*** jsonBody in INdex: " + jsonBody);

                var visitor = await Admin.Services.UserService.GetOrganization(url, _targetResource, _logger, jsonBody);

                if (visitor != null){
                    Console.Write("*** Visitor is not null");
                } else {
                    Console.Write("*** Visitor is null\n");
                }
                
                // return RedirectToPage("/Home/Registration", Organization);
            }
            else
            {
                SigninFailed = true;
            }

            return Page();

        }
    }
}