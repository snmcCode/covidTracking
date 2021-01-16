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
        private IConfiguration _config;
        private LoggerHelper _helper;

        public EventController(IConfiguration config, LoggerHelper helper)
        {
            _config = config;
            _helper = helper;

        }

        public async Task<Event> CreateEvent(Event myEvent)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            Event returnevent = await dbManager.addEvent(myEvent);
            return returnevent;

        }

        public async Task<Event> UpdateEvent(Event myEvent)
        {

            var connectionStr = _config.GetConnectionString("SQLConnectionString");
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, connectionStr, _helper);
            Event returnevent = await dbManager.updateEvent(myEvent);
            return returnevent;

        }


        public async Task<Ticket> bookTicket(Ticket ticket)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            Ticket returnticket = await dbManager.PreregisterToEvent(ticket);
            return returnticket;

        }


        public async Task<List<Event>> getEventsByOrg(int Id, string startDate = "", string endDate = "")
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<Event> myEvents = await dbManager.GetEventsByOrg(Id, startDate, endDate);
            return myEvents;
        }

        public async Task<List<ShortEvent>> getEventsByOrgToday(int Id)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<ShortEvent> myEvents = await dbManager.GetEventsByOrgToday(Id);
            return myEvents;
        }

        public async Task<List<UserEvent>> getEventsByUser(Guid visitorId)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<UserEvent> myEvents = await dbManager.GetEventsByUser(visitorId);
            return myEvents;
        }

        public async Task<bool> checkUserBooking(Guid visitorId, int eventId)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            bool isBooked = await dbManager.CheckUserBooking(eventId, visitorId);
            return isBooked;
        }

        public async Task deleteEvent(int eventId)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            await dbManager.DeleteEvent(eventId);
        }

        public async Task Unregister(UnregisterRequest data)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            await dbManager.UnregisterFromEvent(data.visitorId, data.eventId);
        }


        public async Task groupEvents(List<int> ids)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            await dbManager.GroupEvents(ids);
        }

        public async Task<List<Visitor>> getUsersByEvent(int eventId)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            List<Visitor> visitors = await dbManager.GetUsersByEvent(eventId);
            return visitors;
        }

    }
}
