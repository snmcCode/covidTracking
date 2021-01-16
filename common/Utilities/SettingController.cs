using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.Extensions.Configuration;

namespace Common.Utilities
{
    public class SettingController
    {
        private IConfiguration _config;
        private LoggerHelper _helper;

        public SettingController(IConfiguration config,LoggerHelper helper)
        {
            _config = config;
            _helper = helper;
            
        }
        public async Task<Setting> Get(Setting setting)
        {
            SqlDbManager dbManager = new SqlDbManager(SqlDbManager.DatabaseType.SQL,_config.GetConnectionString("SQLConnectionString"),_helper);
            Setting returnSetting=await dbManager.Settings_Get(setting);
            return returnSetting;
        }
    }
}
