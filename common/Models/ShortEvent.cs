using System;
namespace common.Models
{
//used by the GetEventByOrgToday azure function
    public class ShortEvent
    {
        
        public ShortEvent()
        {
        }
        public int Id;
        public string Name;
        public string Hall;
        public int MinuteOfTheDay;
        public int Capacity;

    }
}
