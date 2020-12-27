using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MasjidTracker.FrontEnd.Models
{
    public enum EventsOrgEnum
        {
            SNMC = 0,
            CIO = 1
        }
    public class EventViewModel
    {
        public List<EventModel> Events { get; set; } // The list of events to display
        public string EventOrg { get; set; }
        public string SearchString { get; set; }

        public List<EventModel> UserEvents {get; set;}

        public Dictionary<string, List<EventModel>> GroupedEvents {get; set;}
    }
}