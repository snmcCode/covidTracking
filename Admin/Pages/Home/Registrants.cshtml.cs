using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Admin.Models;
using System.Security.Claims;
using Admin.Util;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Admin.Services;

namespace Admin.Pages.Home
{
    public class RegistrantsModel : PageModel
    {
        private readonly ILogger<RegistrantsModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        public Dictionary<EventModel, List<VisitorModel>> theBeastDict {get; set;}

         public RegistrantsModel(ILogger<RegistrantsModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }
      
        public async Task<IActionResult> OnGet(string events)
        {
            Console.WriteLine($"I got an event string. It is: {events}");

            var events_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(events);

            theBeastDict = new Dictionary<EventModel, List<VisitorModel>>();

            foreach (string eventid in events_dict.Keys){
                EventModel em = new EventModel();
                em.Id = int.Parse(eventid);
                em.Name = events_dict[eventid];
                Console.WriteLine($"\nEvent Name: {em.Name}");

                List<VisitorModel> vm;

                var url = $"{_config["RETRIEVE_USERS_API_URL"]}/event?eventId={em.Id}";

                vm = await EventsService.GetUsersByEvent(url, _targetResource, _logger);

                Console.WriteLine($"VM Name: {vm.Count}");

                if (vm != null){
                   theBeastDict.Add(em, vm);
                } 
                
            }

            return Page();
        }
    }
}