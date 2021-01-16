using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasjidTracker.FrontEnd.Models;
using FrontEnd;
using System.Threading.Tasks;
using FrontEnd.Models;
using Microsoft.Extensions.Configuration;
using System.Web;
using Common.Utilities;
using System.Runtime.InteropServices;
using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using FrontEnd.Interfaces;

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
        private readonly string _targetResource;

        private readonly int[] orgs = { 1, 2 };

        private List<EventModel> events { get; set; }

        public enum OrgEnum
        {
            SNMC = 1,
            CIO = 2
        }

        public EventsController(IEventsService eventsService, ILogger<EventsController> logger, IConfiguration config)
        {
            this.eventsService = eventsService;
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Events", "Get", path);
            events = await getAllEvents(helper);

            EventViewModel evm = await GetEVM(events, helper);
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
            if (response == 406)
            {
                ViewBag.EventFull = true;
            }

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Events", "Post", path);
            events = await getAllEvents(helper);

            EventViewModel evm = await GetEVM(events, helper);
            return View("Index", evm);

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
            ViewBag.Selected = selection;

            string path = HttpContext.Request.Path;
            LoggerHelper helper = new LoggerHelper(_logger, "Filter Events", "Post", path);

            events = await getAllEvents(helper, selection);

            EventViewModel evm = await GetEVM(events, helper);
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

        private async Task<List<EventModel>> getAllEvents(LoggerHelper helper, string filter = "SNMC")
        {
            // get the value of the selection
            var selection_int = 0;
            List<EventModel> events = new List<EventModel>();
            if (filter == "SNMC")
            {
                selection_int = (int)EventsOrgEnum.SNMC;
                var url = $"{_config["EVENTS_API_URL"]}?orgID={selection_int}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                events = await eventsService.GetEvents(url, _targetResource);
            }
            else if (filter == "CIO")
            {
                selection_int = (int)EventsOrgEnum.CIO;
                var url = $"{_config["EVENTS_API_URL"]}?orgID={selection_int}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                events = await eventsService.GetEvents(url, _targetResource);
            } else { // All
                foreach (int i in orgs)
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

            if (events != null) events.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime));
            return events;
        }

        // Given all the events, gets the user events and the forbidden guids and redirects to the index view with this
        private async Task<EventViewModel> GetEVM(List<EventModel> allEvents, LoggerHelper helper)
        {
            // get the visitor's id
            var v_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier).Trim();
            var user_events_url = $"{_config["USER_EVENTS_API_URL"]}?visitorId={v_id}";
            helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", user_events_url));
            var visitor_events = await eventsService.GetEvents(user_events_url, _targetResource);

            var forbidden_gids = new HashSet<string>();

            if (visitor_events != null) {
                visitor_events.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime));

                foreach (EventModel e in visitor_events)
                {
                    forbidden_gids.Add(e.groupId);
                }
            }

            var eventsView = new EventViewModel
            {
                Events = allEvents,
                UserEvents = visitor_events,
                GroupedEvents = CreateGroupDict(events),
                ForbiddenGuids = forbidden_gids
            };

            return eventsView;
        }
    }
}

