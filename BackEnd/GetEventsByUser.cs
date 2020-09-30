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
    public static class GetEventsByUser
    {
        [FunctionName("GetEventsByUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/user")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Helper helper = new Helper(log, "GetEventsByUser", "GET", "event/user");

            try
            {

                IConfigurationRoot config = new ConfigurationBuilder()
                   .SetBasePath(context.FunctionAppDirectory)
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .Build();


                helper.DebugLogger.LogInvocation();

                Guid visitorId = Guid.Parse(req.Query["visitorId"]);
                if (visitorId.Equals(null))
                {
                    return new NoContentResult();

                }

                EventController Evtctr = new EventController(config, helper);


                var ResponseList = Evtctr.getEventsByUser(visitorId);



                return new OkObjectResult(ResponseList);
            }

            catch (System.FormatException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "Exception";
                helper.DebugLogger.Description = "Input not in correct format";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.BADREQUESTBODY;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                log.LogError(e.Message);
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
