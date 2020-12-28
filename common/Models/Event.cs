using System;
namespace common.Models
{
    public class Event
    {
        public Event()
        { }
        public Event(int OrgId, string Name, DateTime DateTime,string Hall, int Capacity, Boolean IsPrivate,Guid GroupId)
        {
            this.OrgId = OrgId;
            this.Name = Name;
            this.DateTime = DateTime;
            this.Hall = Hall;
            this.Capacity = Capacity;
            this.IsPrivate = IsPrivate;
            this.GroupId = GroupId;
            
        }
        public int Id;
        public int OrgId;
        public string Name;
        public DateTime DateTime;
        public string Hall;
        public int Capacity;
        public bool IsPrivate;
        public int BookingCount;
        public Guid GroupId;

    }
}
