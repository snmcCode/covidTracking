using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Common.Utilities;
using Microsoft.Extensions.Configuration;
using common.Utilities;
using Common.Resources;
using Common.Utilities.Exceptions;

namespace BackEnd
{
    public static class GetUsersByEvent
    {
        [FunctionName("GetUsersByEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/event")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Helper helper = new Helper(log, "GetUsersByEvent", "GET", "users/event");

            try
            {

                IConfigurationRoot config = new ConfigurationBuilder()
                   .SetBasePath(context.FunctionAppDirectory)
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .Build();


                helper.DebugLogger.LogInvocation();

                int eventId;
                if (!int.TryParse(req.Query["eventId"], out eventId))
                {
                    return new NoContentResult();
                }

                EventController Evtctr = new EventController(config, helper);
                var ResponseList = await Evtctr.getUsersByEvent(eventId);

                return new OkObjectResult(ResponseList);

            }
            catch (Exception e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "Exception";
                helper.DebugLogger.Description = "Generic Exception";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.GENERALERROR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                log.LogError(e.Message);
            }
            
            return new ObjectResult(helper.DebugLogger.StatusCodeDescription)
            { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
