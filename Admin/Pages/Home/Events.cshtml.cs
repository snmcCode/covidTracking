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

        public EventsModel(ILogger<EventModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        // Called when page is loaded
        public async Task<ActionResult> OnGetAsync()
        {
            var organization_id = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var url = $"{_config["EVENTS_API_URL"]}/?orgID={organization_id}";
            Events = await EventsService.GetEvents(url, _targetResource, _logger);

            if (Events == null)
            {
                NoneFound = true;
            } else {
                IEnumerable<EventModel> sortedEnum = Events.OrderBy(f=>f.DateTime);
                IList<EventModel> sortedList = sortedEnum.ToList();
                Events = sortedList;
                
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
                    Hall = Event.Hall
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

            var bodyData =
            new
            {
                Id = Event2.Id,
                OrgId = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Name = Event2.Name,
                DateTime = Event2.DateTime,
                Capacity = Event2.Capacity,
                IsPrivate = Event2.IsPrivate,
                Hall = Event2.Hall
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
            Console.WriteLine($"JsonBody: {jsonBody}");
            var response = await EventsService.GroupEvents(url, _targetResource, _logger, jsonBody);
            Console.WriteLine($"\n\nResponse: {response}");

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

            Console.WriteLine($"\n****The event chosen is ");
            
            string[] events = event_ids.Split(',');

            var isSelected = true;
            if (SelectedRows == null || SelectedRows.Count == 0){
                isSelected = false;
            }

            Console.WriteLine($"\n\n***********isSelected: {isSelected}*************");
            var products = new List<bool>
            {
                isSelected
            };
            return new JsonResult(products);
        }
    }
}