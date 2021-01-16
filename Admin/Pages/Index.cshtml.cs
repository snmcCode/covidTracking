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
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;

namespace Admin.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        [BindProperty(SupportsGet = true)]
        [Required]
        public OrgLoginModel OrgLoginModel { get; set; }


        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        public IActionResult OnGet()
        {

            ViewData["SigninFailed"] = false;
            ViewData["ShowLogout"] = false;
            if (User.Identity.IsAuthenticated)
            {
                var id = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var name = HttpContext.User.FindFirstValue(ClaimTypes.Name);

                return RedirectToPage("/Home/Registration", new { Id = id, Name = name });
            }
            return Page();
        }

        // when the sign in button is pressed
        public async Task<IActionResult> OnPost()
        {
            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Signin", "Post", path);
            if (OrgLoginModel.Username != null)
            {
                helper.DebugLogger.LogInvocation();

                var url = $"{_config["AUTH_ADMIN_API_URL"]}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));

                var org = await Admin.Services.UserService.GetOrganization(url, _targetResource, _logger, OrgLoginModel);

                if (org != null)
                {
                    var claims = new List<Claim>{
                        new Claim(ClaimTypes.Name, org.Name),
                        new Claim(ClaimTypes.NameIdentifier, org.Id.ToString()),
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    { };

                    await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                    return RedirectToPage("/Home/Registration", org);
                }
                else
                {
                    ViewData["SigninFailed"] = true;
                }

            }

            return Page();

        }

        // When the log out button is pressed
        public async Task<IActionResult> OnGetLogout()
        {

            await HttpContext.SignOutAsync(
                 CookieAuthenticationDefaults.AuthenticationScheme);

            ViewData["SigninFailed"] = false;
            ViewData["ShowLogout"] = false;
            return RedirectToPage("/Index");
        }
    }
}