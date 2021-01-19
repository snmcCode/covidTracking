using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using common.Models;
using Common.Models;
using Common.Utilities;
using Microsoft.Extensions.Configuration;

namespace common.Utilities
{
    public class OrganizationController
    {
        private IConfiguration _config;
        private LoggerHelper _helper;

        public OrganizationController(IConfiguration config, LoggerHelper helper)
        {
            _config = config;
            _helper = helper;

        }

        public async Task<List<Organization>> GetOrganizations(int? id)
        {
            List<Organization> organizations = new List<Organization>();
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL, _config.GetConnectionString("SQLConnectionString"), _helper);
            organizations = await dbManager.GetOrganizations(id);
            _helper.DebugLogger.LogSuccess();
            return organizations;
        }

    }
}
