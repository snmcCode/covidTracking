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

namespace MasjidTracker.FrontEnd.Controllers
{
    [Route("/events")]
    [Route("Events/[action]")]
    [Authorize]
    public class EventsController : Controller
    {
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

        public EventsController(ILogger<EventsController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            ViewBag.Selected = "All";

            // get the visitor's id
            var v_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // get all listed events
            events = new List<EventModel>();
            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "Events", "Get", path);
            foreach (int i in orgs)
            {
                var url = $"{_config["EVENTS_API_URL"]}?orgID={i}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                var events_by_org = await EventsService.GetEvents(url, _targetResource, _logger);
                if (events_by_org != null)
                {
                    events.AddRange(events_by_org);
                    events.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime));
                }
            }

            // get the events that the user is registered in
            var user_events_url = $"{_config["USER_EVENTS_API_URL"]}?visitorId={v_id}";
            helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", user_events_url));
            var visitor_events = await EventsService.GetEvents(user_events_url, _targetResource, _logger);

            Console.WriteLine($"\n Visitor Events Count: {visitor_events.Count}");

            var forbidden_gids = new HashSet<string>();
            foreach (EventModel e in visitor_events){
                forbidden_gids.Add(e.groupId);
                Console.WriteLine($"Forbidden group ids: {e.groupId}. Count: {forbidden_gids.Count}");
            }

            var eventsView = new EventViewModel
            {
                Events = events,
                UserEvents = visitor_events,
                GroupedEvents = CreateGroupDict(events),
                ForbiddenGuids = forbidden_gids
            };

            return View(eventsView);
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
            var response = await EventsService.RegisterInEvent(url, _targetResource, _logger, jsonBody);

            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> Unregister(String eventid)
        {

            Console.WriteLine($"\n\n **** In Unregister. EventId: {eventid} ******* \n\n");
            
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
            var response = await EventsService.UnregisterFromEvent(url, _targetResource, _logger, jsonBody);
            Console.WriteLine($"\n\n The response is: {response}\n\n");

            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> FilterEvents(string selection){
            
            if (selection == "All"){
                return RedirectToAction("Index");
            }

            ViewBag.Selected = selection;

             // get all listed events
            events = new List<EventModel>();
            string path = HttpContext.Request.Path;
            Helper helper = new Helper(_logger, "Events", "Get", path);

            // get the value of the sele
            var selection_int = 0;
            if (selection == "SNMC") {
                selection_int = (int)EventsOrgEnum.SNMC;
                var url = $"{_config["EVENTS_API_URL"]}?orgID={selection_int}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                events = await EventsService.GetEvents(url, _targetResource, _logger);
            } else if (selection == "CIO") {
                selection_int = (int)EventsOrgEnum.CIO;
                 var url = $"{_config["EVENTS_API_URL"]}?orgID={selection_int}";
                helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", url));
                events = await EventsService.GetEvents(url, _targetResource, _logger);
            } 

            var v_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user_events_url = $"{_config["USER_EVENTS_API_URL"]}?visitorId={v_id}";
            helper.DebugLogger.LogCustomInformation(string.Format("calling backend: {0}", user_events_url));
            var visitor_events = await EventsService.GetEvents(user_events_url, _targetResource, _logger);

            var eventsView = new EventViewModel
            {
                Events = events,
                UserEvents = visitor_events,
                GroupedEvents = CreateGroupDict(events)
            };

            return View("Index", eventsView);

        }

        // put all events in a dictionary with the group id as key
        private Dictionary<string, List<EventModel>> CreateGroupDict(List<EventModel> events){

            Dictionary<string, List<EventModel>> GroupedEvents = new Dictionary<string, List<EventModel>>();

            foreach (EventModel e in events){
                if (GroupedEvents.ContainsKey(e.groupId)){
                    GroupedEvents[e.groupId].Add(e);
                } else {
                    List<EventModel> groupies = new List<EventModel>();
                    groupies.Add(e);
                    GroupedEvents.Add(e.groupId, groupies);
                }
            }
            return GroupedEvents;
        }
    }
}

