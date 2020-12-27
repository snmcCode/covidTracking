using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasjidTracker.FrontEnd.Models
{
    public class EventModel
    {
        public int Id { get; set; }

        public int eventId {get; set;} // because get user events api returns it as eventid.. ask someone to change it later.

        public int OrgId { get; set; }

        public string Name { get; set; }

        public DateTime DateTime {get; set; }

        public int Capacity {get; set;}

        public Boolean IsPrivate {get; set;}

        public string Hall {get; set;}

        public int bookingCount {get; set;}

    }
}
