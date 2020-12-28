using System;
namespace common.Models
{

    public class UserEvent
    {
        public int Id;
        public string Organization;
        public int orgId;
        public string Name;
        public DateTime DateTime;
        public int BookingCount;
        public string groupId;

        public UserEvent()
        {
        }
    }
}
