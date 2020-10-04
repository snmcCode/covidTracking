using System;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Admin.Models;
using Admin.Services;
using System.Collections.Generic;
using System.Linq;
namespace Admin.Pages.Home
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<RegistrationModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        [BindProperty(SupportsGet = true)]
        public VisitorModel Visitor { get; set; }

        public IList<VisitorModel> Visitors { get; set; }

        [ViewData]
        public bool SearchFailed { get; set; }

        [ViewData]
        public bool LoginFailed { get; set; }

        [ViewData]
        public bool NoneFound { get; set; }

        public LoginModel(ILogger<RegistrationModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        public void OnGet()
        {
            SearchFailed = NoneFound = LoginFailed = false;
        }

        public async Task<IActionResult> OnPostVisitorLogin()
        {

            if (Visitor.PhoneNumber !=null && !Visitor.PhoneNumber.StartsWith("+1"))
            {
                Visitor.PhoneNumber = $"+1{Visitor.PhoneNumber}";
            }

            var url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={Visitor.FirstName}&LastName={Visitor.LastName}&PhoneNumber={HttpUtility.UrlEncode(Visitor.PhoneNumber)}";

            var visitors = await UserService.GetUsers(url, _targetResource, _logger);

            if (visitors != null)
            {
                var visitor = visitors[0];
                return RedirectToPage("../Home/View", new { visitor.Id, visitor.FirstName, visitor.LastName });
            }

            LoginFailed = true;
            return Page();
        }

        public async Task<IActionResult> OnPostSearchVisitors()
        {

            // Populate array with the form fields
            string[] info_arr = new string[3];
            info_arr[0] = Visitor.FirstName;
            info_arr[1] = Visitor.LastName;
            info_arr[2] = Visitor.PhoneNumber;


            // 2/3 fields need to be filled in order to search
            int result = info_arr.Count(s => !String.IsNullOrEmpty(s));

            if (result < 2)
            {
                SearchFailed = true;
                return Page();
            }

            // Determine which of the fields is missing
            var null_item = 0;

            for (int i = 0; i < 3; i++)
            {
                if (String.IsNullOrEmpty(info_arr[i]))
                {
                    null_item = i;
                    break;
                }
            }

            // Get the list of users that match the fields
            var url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={info_arr[0]}&LastName={info_arr[1]}&PhoneNumber={HttpUtility.UrlEncode(info_arr[2])}";
            switch (null_item)
            {
                case 0: // First Name is missing
                    info_arr[2] = $"+1{Request.Form["phoneNumber"]}";
                    url = $"{_config["RETRIEVE_USERS_API_URL"]}?LastName={info_arr[1]}&PhoneNumber={HttpUtility.UrlEncode(info_arr[2])}";
                    break;
                case 1: // Last Name is missing
                    info_arr[2] = $"+1{Request.Form["phoneNumber"]}";
                    url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={info_arr[0]}&PhoneNumber={HttpUtility.UrlEncode(info_arr[2])}";
                    break;
                case 2: // Phone Number is missing
                    url = $"{_config["RETRIEVE_USERS_API_URL"]}?FirstName={info_arr[0]}&LastName={info_arr[1]}";
                    break;
            }

            Visitors = await UserService.GetUsers(url, _targetResource, _logger);

            if (Visitors == null)
            {
                NoneFound = true;
            }

            return Page();
        }

        public IActionResult OnPostSelect()
        {
            return RedirectToPage("/Home/View", new { Visitor.Id, Visitor.FirstName, Visitor.LastName });
        }
    }
}