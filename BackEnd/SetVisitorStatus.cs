using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Common.Resources;
using Microsoft.Extensions.Configuration;
using Common.Utilities;
using common.Utilities;
using Microsoft.Extensions.Options;
using common.Models;

namespace BackEnd
{
    public class SetVisitorStatus
    {
        private readonly IConfiguration config;
        
        public SetVisitorStatus(IConfiguration config )
        {
            this.config = config;
        }

        [FunctionName("SetVisitorStatus")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "Status/User/")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            LoggerHelper helper = new LoggerHelper(log, "SetVisitorStatus", "PUT", "Status/User");
            try
            {
                helper.DebugLogger.LogInvocation();
                using (var streamReader = new StreamReader(req.Body))
                {
                    helper.DebugLogger.RequestBody = await streamReader.ReadToEndAsync();
                }
                helper.DebugLogger.LogRequestBody();
                VisitorStatus visitorStatus = JsonConvert.DeserializeObject<VisitorStatus>(helper.DebugLogger.RequestBody);
                VisitorController visitorController = new VisitorController(config, helper);
                bool result = await visitorController.SetVisitorStatus(visitorStatus);

                return new OkObjectResult(result);
            }
            catch(Exception e)
            {
                log.LogError(e.ToString());
                return new ConflictObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = 500 };
            }
            
        }
    }
}
