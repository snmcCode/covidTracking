using System;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Admin.Models;
using Admin.Services;
using Admin.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Common.Utilities;
using System.Security.Claims;

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
        public bool MissingFields { get; set; }

        [ViewData]
        public bool NoneFound { get; set; }

        private readonly ICacheableService _cacheableService;

        public LoginModel(ILogger<RegistrationModel> logger, IConfiguration config, ICacheableService cacheableService)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
            _cacheableService = cacheableService;
        }

        public void OnGet()
        {
            SearchFailed = NoneFound = MissingFields = LoginFailed = false;
        }

        public async Task<IActionResult> OnPostVisitorLogin()
        {

            if (NumberOfFilledFields(FormFieldsArray()) < 3)
            {
                MissingFields = true;
                return Page();
            }

            if (!Visitor.PhoneNumber.StartsWith("+1"))
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

        public int NumberOfFilledFields(string[] arr)
        {

            int result = arr.Count(s => !String.IsNullOrEmpty(s));

            return result;
        }

        public string[] FormFieldsArray()
        {
            string[] info_arr = new string[3];
            info_arr[0] = Visitor.FirstName;
            info_arr[1] = Visitor.LastName;
            info_arr[2] = Visitor.PhoneNumber;

            return info_arr;
        }

        public async Task<IActionResult> OnPostSearchVisitors()
        {
            // Populate array with the form fields
            string[] info_arr = FormFieldsArray();

            // 2/3 fields need to be filled in order to search
            int result = NumberOfFilledFields(info_arr);

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
            await DescribeUserStatus();

            if (Visitors == null)
            {
                NoneFound = true;
            }

            LogSearch();

            return Page();
        }

        // Log every search made and the org responsible for it.
        public void LogSearch()
        {
            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Search Visitors", "Post", path);
            var org_name = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            helper.DebugLogger.LogCustomInformation(
                        $@"The search was done by {org_name}. They searched for the following fields: 
                        FirstName: {Visitor.FirstName} 
                        LastName: {Visitor.LastName} 
                        PhoneNumber: {Visitor.PhoneNumber}");
        }

        public IActionResult OnPostSelect()
        {
            return RedirectToPage("/Home/View", new { Visitor.Id, Visitor.FirstName, Visitor.LastName });
        }

        private async Task DescribeUserStatus()
        {

            var status_url = $"{_config["GET_STATUSES_API_URL"]}";
            var Statuses = await _cacheableService.GetStatuses(status_url, _targetResource);
            Dictionary<int, string> StatusDict = Statuses.ToDictionary(x => x.BitValue, x => x.Name);
            var orgId = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));


            foreach (VisitorModel visitor in Visitors)
            {
                // get & set visitor status 
                var url = $"{_config["SET_VISITOR_STATUS_URL"]}orgId={orgId}&visitorId={visitor.Id}";
                var response = await UserService.GetUserStatus(url, _targetResource, _logger);
                visitor.status = response;

                if (response != 0)
                {
                    List<int> visitor_statuses = ViewModel.DecomposedStatuses(response);
                    List<string> status_names = visitor_statuses.Where(k => StatusDict.ContainsKey(k)).Select(k => StatusDict[k]).ToList<string>();

                    // set the string
                    visitor.statusNames = status_names;
                }
            }
        }
    }
}