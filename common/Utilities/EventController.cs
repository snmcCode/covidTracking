using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<Event> CreateEvent(Event myEvent)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            Event returnevent = await dbManager.addEvent(myEvent);
            return returnevent;

        }

        public async Task<Event> UpdateEvent(Event myEvent)
        {

            var connectionStr = _config.GetConnectionString("SQLConnectionString");
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, connectionStr, _helper);
            Event returnevent = await dbManager.updateEvent(myEvent);
            return returnevent;

        }


        public async Task<Ticket> bookTicket(Ticket ticket)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            Ticket returnticket = await dbManager.PreregisterToEvent(ticket);
            return returnticket;

        }


        public async Task<List<Event>> getEventsByOrg(int Id, string startDate="",string endDate= "")
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<Event> myEvents = await dbManager.GetEventsByOrg(Id,startDate,endDate);
            return myEvents;
        }

        public async Task<List<ShortEvent>> getEventsByOrgToday(int Id)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<ShortEvent> myEvents = await dbManager.GetEventsByOrgToday(Id);
            return myEvents;
        }

        public async Task<List<UserEvent>> getEventsByUser(Guid visitorId)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<UserEvent> myEvents = await dbManager.GetEventsByUser(visitorId);
            return myEvents;
        }


        public async void deleteEvent(int eventId)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            dbManager.DeleteEvent(eventId);
        }

        public async void Unregister(UnregisterRequest data)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            dbManager.UnregisterFromEvent(data.visitorId, data.eventId);
        }


        public async void groupEvents(List<int> ids)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            dbManager.GroupEvents(ids);
        }

    }
}
