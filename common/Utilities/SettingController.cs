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
        private IConfigurationRoot _config;
        private Helper _helper;

        public SettingController(IConfigurationRoot config,Helper helper)
        {
            _config = config;
            _helper = helper;
            
        }
        public async Task<Setting> Get(Setting setting)
        {
            DbManager dbManager = new DbManager(DbManager.DatabaseType.SQL,_config.GetConnectionString("SQLConnectionString"),_helper);
            Setting returnSetting=await dbManager.Settings_Get(setting);
            return returnSetting;
        }
    }
}
