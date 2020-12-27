using System;
namespace common.Models
{

    public class UserEvent
    {
        public int EventId;
        public string Organization;
        public string Name;
        public DateTime DateTime;
        public int BookingCount;

        public UserEvent()
        {
        }
    }
}
