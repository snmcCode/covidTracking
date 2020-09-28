using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Admin.Models;
using Admin.Services;

namespace Admin.Pages.Home
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<RegistrationModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        [BindProperty(SupportsGet = true)]
        public VisitorModel Visitor { get; set; }

        public LoginModel(ILogger<RegistrationModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        public async Task<IActionResult> OnPostVisitorLogin()
        {

            var firstName = Request.Form["firstName"];
            var lastName = Request.Form["lastName"];
            var phoneNumber = $"+1{Request.Form["phoneNumber"]}";

            var url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={firstName}&LastName={lastName}&PhoneNumber={HttpUtility.UrlEncode(phoneNumber)}";

            var visitor = await UserService.GetUsers(url, _targetResource, _logger);

            if (visitor != null)
            {
                return RedirectToPage("../Home/View", new { visitor.Id, visitor.FirstName, visitor.LastName });
            }

            ViewData["LoginFailed"] = true;
            return Page();
        }
    }
}