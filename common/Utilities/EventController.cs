using System;
using System.Collections.Generic;
using common.Models;
using Common.Models;
using Common.Utilities;
using Microsoft.Extensions.Configuration;


namespace common.Utilities
{
    public class EventController
    {
        private IConfigurationRoot _config;
        private Helper _helper;

        public EventController(IConfigurationRoot config, Helper helper)
        {
            _config = config;
            _helper = helper;

        }

        public  Event CreateEvent(Event myEvent)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            Event returnevent = dbManager.addEvent(myEvent);
            return returnevent;
           
        }

        public Event UpdateEvent(Event myEvent)
        {

            var connectionStr = _config.GetConnectionString("SQLConnectionString");
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL,connectionStr, _helper);
            Event returnevent = dbManager.updateEvent(myEvent);
            return returnevent;

        }


        public Ticket bookTicket(Ticket ticket)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            Ticket returnticket = dbManager.PreregisterToEvent(ticket);
            return returnticket;

        }


        public List<Event> getEventsByOrg(int Id)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<Event> myEvents = dbManager.GetEventsByOrg(Id);
            return myEvents;
        }


    }
}
