using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Admin.Models;
using Admin.Services;
using System.Collections.Generic;
using System.Security.Claims;
using Newtonsoft.Json;
using System.Linq;
using common.Models;
using System.Collections;

namespace Admin.Pages.Home
{
    public class EventsModel : PageModel
    {
        private readonly ILogger<EventModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        public IList<EventModel> Events { get; set; }

        [ViewData]
        public bool NoneFound { get; set; }

        [BindProperty]
        public EventModel Event { get; set; }

        [BindProperty]
        public EventModel Event2 { get; set; } // the event bound to the update form

        [BindProperty]
        public List<string> SelectedRows { get; set; }

        [BindProperty]
        public List<int> SelectedAudiences { get; set; } // the list of selected special audiences

        [BindProperty]
        public List<int> SelectedAudiencesModal { get; set; } // the list of selected special audiences

        public List<StatusInfo> Statuses { get; set; }

        public Dictionary<int, string> StatusDict { get; set; }

        public EventsModel(ILogger<EventModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        // Called when page is loaded
        public async Task<ActionResult> OnGetAsync()
        {
            // Get all events
            var organization_id = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var events_url = $"{_config["EVENTS_API_URL"]}?orgID={organization_id}";
            Events = await EventsService.GetEvents(events_url, _targetResource, _logger);

            // Get all statuses
            var status_url = $"{_config["GET_STATUSES_API_URL"]}";
            Statuses = await EventsService.GetStatuses(status_url, _targetResource, _logger);

            // Add the 'none' status
            StatusInfo noneStatus = new StatusInfo();
            noneStatus.BitValue = 0;
            noneStatus.Name = "none";
            Statuses.Add(noneStatus);

            // Convert to dictionary. r we using this?
            StatusDict = Statuses.ToDictionary(x => x.BitValue, x => x.Name);

            if (Events == null)
            {
                NoneFound = true;
            }
            else
            {
                IEnumerable<EventModel> sortedEnum = Events.OrderBy(f => f.DateTime);
                IList<EventModel> sortedList = sortedEnum.ToList();
                Events = sortedList;

                foreach (EventModel e in Events)
                {
                    if (e.TargetAudience != null) e.decomposedTarget = GetStatusListNames((int)e.TargetAudience, Statuses);
                }

            }
            return Page();
        }

        public async Task<IActionResult> OnPostCreateEvent()
        {
            var url = $"{_config["EVENTS_API_URL"]}/";
            var bodyData =
                new
                {
                    OrgId = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    Name = Event.Name,
                    DateTime = Event.DateTime,
                    Capacity = Event.Capacity,
                    IsPrivate = Event.IsPrivate,
                    Hall = Event.Hall,
                    TargetAudience = getTargetAudValue(SelectedAudiences)
                };
            string jsonBody = JsonConvert.SerializeObject(bodyData);

            var response = await EventsService.CreateEvent(url, _targetResource, _logger, jsonBody);
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDelete()
        {
            var url = $"{_config["EVENTS_API_URL"]}";

            var bodyData =
               new
               {
                   Id = Event.Id
               };
            string jsonBody = JsonConvert.SerializeObject(bodyData);
            var response = await EventsService.DeleteEvent(url, _targetResource, _logger, jsonBody);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateEvent()
        {

            var url = $"{_config["EVENTS_API_URL"]}";

            Event2.TargetAudience = getTargetAudValue(SelectedAudiencesModal);
            var bodyData =
            new
            {
                Id = Event2.Id,
                OrgId = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Name = Event2.Name,
                DateTime = Event2.DateTime,
                Capacity = Event2.Capacity,
                IsPrivate = Event2.IsPrivate,
                Hall = Event2.Hall,
                TargetAudience = Event2.TargetAudience
            };

            string jsonBody = JsonConvert.SerializeObject(bodyData);
            var response = await EventsService.UpdateEvent(url, _targetResource, _logger, jsonBody);
            await OnGetAsync();
            return RedirectToPage();
        }

        /*
         Called when 'group' is clicked. 
        */
        public async Task<IActionResult> OnPostGroup()
        {
            var url = $"{_config["EVENTS_API_URL"]}";
            Dictionary<string, int> bodydic = new Dictionary<string, int>();
            int i = 1;
            foreach (string row in SelectedRows)
            {
                var idname = "id" + i++;
                bodydic.Add(idname, Int32.Parse(row));
            }

            string jsonBody = JsonConvert.SerializeObject(bodydic);
            var response = await EventsService.GroupEvents(url, _targetResource, _logger, jsonBody);

            return RedirectToPage();
        }

        /*
            Gets all events and then filters them.
            There must be a better way to do this...
        */
        public async Task<IActionResult> OnPostFilterEvents(DateTime startdate, DateTime enddate)
        {
            // convert to string in format used by API
            var start = startdate.ToString("yyyy-MM-dd");
            var end = enddate.ToString("yyyy-MM-dd");

            // handle unspecified fields
            if (startdate == DateTime.MinValue)
            {
                start = "";
            }
            if (enddate == DateTime.MinValue)
            {
                end = "";
            }

            var organization_id = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var url = $"{_config["EVENTS_API_URL"]}/?orgID={organization_id}&startDate={start}&endDate={end}";
            Events = await EventsService.GetEvents(url, _targetResource, _logger);

            if (Events == null)
            {
                NoneFound = true;
            }

            return Page();
        }

        public IActionResult OnPostResetFilter()
        {
            return RedirectToPage();
        }

        public IActionResult OnGetRegistrants(string event_ids)
        {
            string[] events = event_ids.Split(',');

            var isSelected = true;
            if (SelectedRows == null || SelectedRows.Count == 0)
            {
                isSelected = false;
            }

            var products = new List<bool>
            {
                isSelected
            };
            return new JsonResult(products);
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

        // given a status id, return it's corresponding name
        private string GetStatusName(int status_id, List<StatusInfo> statuses)
        {

            StatusInfo target = statuses.Where(s => s.BitValue == status_id).First();

            var name = target == null ? "" : target.Name;

            return name;
        }


        /* Factorizes / decomposes an integer into bits. 
            Returns a dictionary of the status and their int value for the given int value. */
        public Dictionary<int, string> GetStatusListNames(int int_val, List<StatusInfo> statuses)
        {
            // Initialize the dictionary of int_val's statuses (this will be a subset of GetStatuses)
            Dictionary<int, string> relevant_statuses = new Dictionary<int, string>();

            if (int_val == 0){
                relevant_statuses[0] = "none";
                return relevant_statuses;
            }

            // convert int to binary
            BitArray b = new BitArray(new int[] { int_val });
            bool[] bits = new bool[b.Count];
            b.CopyTo(bits, 0);

            // convert boolean values in bit array to 0s and 1s
            byte[] bitValues = bits.Select(bit => (byte)(bit ? 1 : 0)).ToArray();

            int pos = 0;
            foreach (byte bit in bitValues)
            {
                // get the integer representation of the bit value
                int dec_value = (int)Math.Pow(2, pos++);
                if (bit != 0)
                {
                    relevant_statuses.Add(dec_value, GetStatusName(dec_value, statuses));
                }
            }

            return relevant_statuses;
        }
    }
}