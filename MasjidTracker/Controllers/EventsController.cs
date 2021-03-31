using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasjidTracker.FrontEnd.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Common.Utilities;
using Common.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Newtonsoft.Json;
using FrontEnd.Interfaces;
using System.Collections;
using System.Linq;

namespace MasjidTracker.FrontEnd.Controllers
{
    [Route("/events")]
    [Route("Events/[action]")]
    [Authorize]
    public class EventsController : Controller
    {
        private readonly IEventsService eventsService;
        private readonly ILogger<EventsController> _logger;
        private readonly IConfiguration _config;
        private readonly ICacheableService _cacheableService;
        private readonly string _targetResource;

        private List<EventModel> events { get; set; }

        public EventsController(IEventsService eventsService, ILogger<EventsController> logger, ICacheableService cacheableService, IConfiguration config)
        {
            this.eventsService = eventsService;
            _logger = logger;
            _config = config;
            _cacheableService = cacheableService;
            _targetResource = config["TargetAPIAzureADAPP"];

        }

        [HttpGet]
        public async Task<IActionResult> Index(string error = null)
        {

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Events", "Get", path);
            ViewBag.Announcement = await GetAnnouncement();
            events = await getAllEvents(helper);

            EventViewModel evm = await GetEVM(events, helper, error);
            return View(evm);

        }

        [HttpPost]
        public async Task<IActionResult> Register(String eventid)
        {
            // get the visitor's id
            var v_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var url = $"{_config["EVENTS_API_URL"]}/booking";

            var bodyData =
               new
               {
                   visitorId = v_id,
                   eventId = eventid
               };
            string jsonBody = JsonConvert.SerializeObject(bodyData);
            var response = await eventsService.RegisterInEvent(url, _targetResource, jsonBody);

            // status code == 406 means capacity is full. Unsuccessful registration. 
            string errorMsg = null;
            if (response == 406)
            {
                errorMsg = "Sorry, you cannot register for this event. It filled up while you were on this page.";
            }
            if (response == 418)
            {
                errorMsg = "Sorry, you cannot register for this event. It is intended for a specific audience only.";
            }

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Events", "Post", path);
            return RedirectToAction("Index", new { error = errorMsg });

        }

        [HttpPost]
        public async Task<IActionResult> Unregister(String eventid)
        {
            // get the visitor's id
            var v_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var url = $"{_config["EVENTS_API_URL"]}/booking";

            var bodyData =
               new
               {
                   visitorId = v_id,
                   eventId = eventid
               };
            string jsonBody = JsonConvert.SerializeObject(bodyData);
            var response = await eventsService.UnregisterFromEvent(url, _targetResource, jsonBody);

            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> FilterEvents(string selection)
        {
            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Filter Events", "Post", path);

            events = await getAllEvents(helper, selection);

            EventViewModel evm = await GetEVM(events, helper, null, selection);
            return View("Index", evm);
        }

        // put all events in a dictionary with the group id as key
        private Dictionary<string, List<EventModel>> CreateGroupDict(List<EventModel> events)
        {

            Dictionary<string, List<EventModel>> GroupedEvents = new Dictionary<string, List<EventModel>>();

            foreach (EventModel e in events)
            {
                if (GroupedEvents.ContainsKey(e.groupId))
                {
                    GroupedEvents[e.groupId].Add(e);
                }
                else
                {
                    List<EventModel> groupies = new List<EventModel>();
                    groupies.Add(e);
                    GroupedEvents.Add(e.groupId, groupies);
                }
            }
            return GroupedEvents;
        }

        // TO DO: There must be a better way to do this...
        private async Task<List<EventModel>> getAllEvents(LoggerHelper helper, string filter = "SNMC")
        {
            // get the value of the selection
            List<EventModel> events = new List<EventModel>();

            Dictionary<int, string> orgs = await GetOrganizations();
            var name_keyed = orgs.ToDictionary(x => x.Value, x => x.Key); // swap keys and values. 

            if (name_keyed.ContainsKey(filter))
            {
                var filter_id = name_keyed[filter];
                var url = $"{_config["EVENTS_API_URL"]}?orgID={filter_id}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                events = await eventsService.GetEvents(url, _targetResource);
            }
            else
            { // All
                foreach (int i in orgs.Keys)
                {
                    var url = $"{_config["EVENTS_API_URL"]}?orgID={i}";
                    helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                    var events_by_org = await eventsService.GetEvents(url, _targetResource);
                    if (events_by_org != null)
                    {
                        events.AddRange(events_by_org);
                    }
                }
            }

            if (events != null)
            {
                events.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime)); // sort by datetime
                // Decompose targetAud
                foreach (EventModel e in events)
                {
                    if (e.targetAudience != 0) e.decomposedTarget = await GetStatusListNames(e.targetAudience);
                }
            }
            return events;
        }

        // Given all the events, gets the user events and the forbidden guids and redirects to the index view with this
        private async Task<EventViewModel> GetEVM(List<EventModel> allEvents, LoggerHelper helper, string errorMsg = null, string filter = "SNMC")
        {
            // get the visitor's id
            var v_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier).Trim();
            var user_events_url = $"{_config["USER_EVENTS_API_URL"]}?visitorId={v_id}";
            helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", user_events_url));
            var visitor_events = await eventsService.GetEvents(user_events_url, _targetResource);

            var forbidden_gids = new HashSet<string>();

            if (visitor_events != null)
            {
                visitor_events.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime));

                foreach (EventModel e in visitor_events)
                {
                    forbidden_gids.Add(e.groupId);
                }
            }

            var orgs_dict = await GetOrganizations();
            var eventsView = new EventViewModel
            {
                Events = allEvents,
                UserEvents = visitor_events,
                GroupedEvents = CreateGroupDict(events),
                ForbiddenGuids = forbidden_gids,
                ErrorMessage = errorMsg,
                Organizations = orgs_dict,
                SelectedOrg = filter
            };

            return eventsView;
        }

        internal async Task<string> GetAnnouncement()
        {

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "getAnnouncement", "Get", path);
            string cururl = HttpContext.Request.Host.ToString();
            Common.Models.Setting mysetting = new Common.Models.Setting(cururl, "eventAnnouncement");
            string url = $"{_config["RETRIEVE_SETTINGS"]}?domain={mysetting.domain}&key={mysetting.key}";
            string announcement = await _cacheableService.GetSetting(url, mysetting.domain, mysetting.key, _targetResource, mysetting);
            return announcement;
        }


        private async Task<List<StatusModel>> GetStatuses()
        {

            var url = $"{_config["GET_STATUSES_API_URL"]}";
            var statuses = await _cacheableService.GetStatuses(url, _targetResource);

            return statuses;
        }


        // given a status id, return it's corresponding name
        private async Task<string> GetStatusName(int status_id)
        {
            List<StatusModel> all_statuses = await GetStatuses();

            StatusModel target = all_statuses.Where(s => s.bitValue == status_id).First();

            var name = target == null ? "" : target.name;

            return name;
        }


        /* Factorizes / decomposes an integer into bits. 
            Returns a dictionary of the status and their int value for the given int value. */
        public async Task<Dictionary<int, string>> GetStatusListNames(int int_val)
        {

            // convert int to binary
            BitArray b = new BitArray(new int[] { int_val });
            bool[] bits = new bool[b.Count];
            b.CopyTo(bits, 0);

            // convert boolean values in bit array to 0s and 1s
            byte[] bitValues = bits.Select(bit => (byte)(bit ? 1 : 0)).ToArray();

            // Initialize the dictionary of int_val's statuses (this will be a subset of GetStatuses)
            Dictionary<int, string> relevant_statuses = new Dictionary<int, string>();

            int pos = 0;
            foreach (byte bit in bitValues)
            {
                // get the integer representation of the bit value
                int dec_value = (int)Math.Pow(2, pos++);
                if (bit != 0)
                {
                    relevant_statuses.Add(dec_value, await GetStatusName(dec_value));
                }

            }

            return relevant_statuses;
        }


        public async Task<Dictionary<int, string>> GetOrganizations()
        {

            var url = $"{_config["GET_ORGS_API_URL"]}";
            List<Organization> orgs = await _cacheableService.GetOrgs(url, _targetResource);

            Dictionary<int, string> orgs_dict = new Dictionary<int, string>();
            orgs_dict = orgs.ToDictionary(x => x.Id, x => x.Name); // TO DO: Store this dictionary in cache, instead of the serialized list

            return orgs_dict;
        }

    }
}

