using Common.Models;
using System.Collections.Generic;

namespace MasjidTracker.FrontEnd.Models
{
    public class EventViewModel
    {
        public List<EventModel> Events { get; set; } // The list of events to display
        
        public List<EventModel> UserEvents {get; set;}

        public Dictionary<string, List<EventModel>> GroupedEvents {get; set;}

        public HashSet<string> ForbiddenGuids {get; set;} // These are the events that the user can't register for

        public string ErrorMessage {get; set;}

        public Dictionary<int, string> Organizations {get; set;}

        public string SelectedOrg {get; set;}
    }
}