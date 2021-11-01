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
    public class GetVisitorStatus
    {
        private readonly IConfiguration config;

        public GetVisitorStatus(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("GetVisitorStatus")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Status/User/")] HttpRequest req,
            ILogger log)
        {
            LoggerHelper helper = new LoggerHelper(log, "GetVisitorStatus", "GET", "Status/User");

            try
            {
                helper.DebugLogger.LogInvocation();
                
                int orgId = int.Parse(req.Query["orgId"]);
                Guid visitorId = Guid.Parse(req.Query["visitorId"]);

                if (orgId.Equals(null) | visitorId.Equals(null))
                {
                    return new NoContentResult();
                }

                VisitorController visitorController = new VisitorController(config, helper);
                int result = await visitorController.GetVisitorStatus(orgId, visitorId);

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                throw e;
            }
        }
    }
}
