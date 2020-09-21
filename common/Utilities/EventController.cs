using System;
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


    }
}
