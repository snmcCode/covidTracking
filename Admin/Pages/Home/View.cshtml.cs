using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Admin.Models;
using Admin.Util;
using Admin.Services;
using Admin.Interfaces;
using common.Models;
using Common.Utilities;
using Newtonsoft.Json;

namespace Admin.Pages.Home
{
    public class ViewModel : PageModel
    {
        private readonly ILogger<ViewModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        [BindProperty(SupportsGet = true)]
        public string printTitle { get; set; }

        [BindProperty]
        public VisitorModel Visitor { get; set; }

        public List<StatusInfo> Statuses { get; set; }

        public Dictionary<int, string> StatusDict { get; set; }

        [BindProperty]
        public List<int> SelectedAudiencesModal { get; set; } // the list of selected special audiences

        [TempData]
        public bool SetStatusResult { get; set; }

        private readonly ICacheableService _cacheableService;

        public ViewModel(ILogger<ViewModel> logger, IConfiguration config, ICacheableService cacheableService)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
            _cacheableService = cacheableService;
        }
        public async Task<IActionResult> OnGet(VisitorModel visitor)
        {
            if (visitor == null)
            {
                return RedirectToRoute("/Home/Home");
            }
            Visitor = visitor;

            Visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(Visitor.Id.ToString());
            Visitor.status = await GetStatus();
            Visitor.decomposedStatuses = DecomposedStatuses(Visitor.status);

            var status_url = $"{_config["GET_STATUSES_API_URL"]}";
            Statuses = await _cacheableService.GetStatuses(status_url, _targetResource);
            StatusDict = Statuses.ToDictionary(x => x.BitValue, x => x.Name);

            return Page();
        }

        // Called when The Register Another button is pressed
        public IActionResult OnPostRegisterAnother()
        {
            var id = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var name = HttpContext.User.FindFirstValue(ClaimTypes.Name);

            return RedirectToPage("/Home/Registration", new { Id = id, Name = name });
        }

        public IActionResult OnPostLoginAnother()
        {
            return RedirectToPage("/Home/Login");
        }

        public async Task<IActionResult> OnPostSetStatuses()
        {

            int status = getTargetAudValue(SelectedAudiencesModal);

            var url = $"{_config["SET_VISITOR_STATUS_URL"]}";

            var bodyData =
                new
                {
                    visitorId = Visitor.Id,
                    orgId = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    statusValue = status
                };

            string jsonBody = JsonConvert.SerializeObject(bodyData);
            try
            {
                var response = await UserService.SetUserStatus(url, _targetResource, _logger, jsonBody);
                SetStatusResult = true;
            }
            catch (Exception e)
            {
                SetStatusResult = false;

                LoggerHelper helper = new LoggerHelper(_logger, "SetStatuses", null, "ViewModel/OnPostSetStatuses");
                helper.DebugLogger.LogCustomError($"Error setting user status. Exception: {e}");
            }

            return RedirectToPage("../Home/View", new { Visitor.Id, Visitor.FirstName, Visitor.LastName });
        }


        private int getTargetAudValue(List<int> selection)
        {
            int all_auds = 0;
            // check if any special audiences
            foreach (int audience in selection)
            {
                all_auds = all_auds | audience;
            }

            return (int)all_auds;
        }


        private async Task<int> GetStatus()
        {

            var orgId = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var url = $"{_config["SET_VISITOR_STATUS_URL"]}orgId={orgId}&visitorId={Visitor.Id}";

            var response = await UserService.GetUserStatus(url, _targetResource, _logger);

            return response;
        }


        public static List<int> DecomposedStatuses(int status)
        {
            BitArray b = new BitArray(new int[] { status });
            bool[] bits = new bool[b.Count];
            b.CopyTo(bits, 0);

            // convert boolean values in bit array to 0s and 1s
            byte[] bitValues = bits.Select(bit => (byte)(bit ? 1 : 0)).ToArray();

            // Initialize the dictionary of int_val's statuses (this will be a subset of GetStatuses)
            List<int> decomposed = new List<int>();

            int pos = 0;
            foreach (byte bit in bitValues)
            {
                // get the integer representation of the bit value
                int dec_value = (int)Math.Pow(2, pos++);
                if (bit != 0)
                {
                    decomposed.Add(dec_value);
                }

            }

            return decomposed;

        }
        
    }
}