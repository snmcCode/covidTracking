using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasjidTracker.FrontEnd.Models
{
    public class EventModel
    {
        public int Id { get; set; }

        public string organization { get; set; }

        public int OrgId { get; set; }

        public string Name { get; set; }

        public DateTime DateTime { get; set; }

        public int Capacity { get; set; }

        public Boolean IsPrivate { get; set; }

        public string Hall { get; set; }

        public int bookingCount { get; set; }

        public string groupId { get; set; }

        public int targetAudience { get; set; }

        public Dictionary<int, string> decomposedTarget {get; set;} // could also make this just a string, or a list of strings

    }
}
