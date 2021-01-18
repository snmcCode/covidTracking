using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasjidTracker.FrontEnd.Models
{
    [Flags]
    public enum VisitorsAttributes
    {
        none = 0,
        senior = 1,
        member = 2,
        board = 4,
        youth = 8
    };
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

    }
}
