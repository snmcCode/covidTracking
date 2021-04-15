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
    public class RetrieveSettings
    {
        private readonly IConfiguration config;

        public RetrieveSettings(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("RetrieveSettings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "setting")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            LoggerHelper helper = new LoggerHelper(log, "RetrieveSettings", "Get", "setting");

            try
            {
                helper.DebugLogger.LogInvocation();

                string settingDomain = req.Query["domain"];
                string settingKey = req.Query["key"];

                if (string.IsNullOrEmpty(settingDomain) | string.IsNullOrEmpty(settingKey))
                {
                    return new NoContentResult();
                }
                helper.DebugLogger.LogCustomInformation($"Requested setting key {settingKey} for domain {settingDomain}");
                Setting setting = new Setting(settingDomain, settingKey);
                SettingController settingcontroller = new SettingController(config, helper);
                Setting returnsetting = await settingcontroller.Get(setting);
                helper.DebugLogger.LogCustomInformation($"Setting value is {returnsetting.value}");
                if (!string.IsNullOrEmpty(returnsetting.value))
                    return new OkObjectResult(returnsetting);
                else
                {
                    helper.DebugLogger.LogCustomInformation($"Setting value is null or empty string");
                    return new NotFoundResult();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new ConflictObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = 500 };
            }

        }
    }
}
