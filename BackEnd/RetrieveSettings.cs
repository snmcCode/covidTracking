using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using Common.Models;
using Common.Resources;
using Common.Utilities;
using Common.Utilities.Exceptions;

namespace BackEnd
{
    public static class RetrieveSettings
    {
        [FunctionName("RetrieveSettings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "setting")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            try
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

                Helper helper = new Helper(log, "RetrieveSettings", "Get", "setting");

                helper.DebugLogger.LogInvocation();

                string settingDomain = req.Query["domain"];
                string settingKey = req.Query["key"];

                if (string.IsNullOrEmpty(settingDomain) | string.IsNullOrEmpty(settingKey))
                {
                    return new NoContentResult();
                }

                Setting setting = new Setting(settingDomain, settingKey);
                SettingController settingcontroller = new SettingController(config, helper);
                Setting returnsetting = await settingcontroller.Get(setting);

                if (!string.IsNullOrEmpty(returnsetting.value))
                    return new OkObjectResult(returnsetting);
                else
                    return new NotFoundResult();
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
            
        }
    }
}
