using System;
using System.Collections.Generic;
using System.Text;
using Common.Models;
using Common.Utilities;
using Microsoft.Extensions.Configuration;
using common.Models;
using System.Threading.Tasks;

namespace common.Utilities
{
    public class VisitorController
    {
        private IConfiguration _config;
        private LoggerHelper _helper;

        public VisitorController(IConfiguration config,LoggerHelper helper)
        {
            _config = config;
            _helper = helper;
        }

        public async Task<bool>SetVisitorStatus(VisitorStatus visitorStatus)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            bool result= await dbManager.SetVisitorStatus(visitorStatus);
            return result;
        }
    }
}
