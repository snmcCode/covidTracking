using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MasjidTracker.FrontEnd.Models
{
    public enum EventsOrgEnum
        {
            SNMC = 1,
            CIO = 2
        }
    public class EventViewModel
    {
        public List<EventModel> Events { get; set; } // The list of events to display
        
        public List<EventModel> UserEvents {get; set;}

        public Dictionary<string, List<EventModel>> GroupedEvents {get; set;}

        public HashSet<string> ForbiddenGuids {get; set;} // These are the events that the user can't register for
    }
}