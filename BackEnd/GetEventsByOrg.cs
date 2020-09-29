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

namespace BackEnd
{
    public static class GetEventsByOrg
    {
        [FunctionName("GetEventsByOrg")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",Route = "ticket")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Helper helper = new Helper(log, "RegisterToEvent", "POST", "tickets");

            try
            {

                IConfigurationRoot config = new ConfigurationBuilder()
                   .SetBasePath(context.FunctionAppDirectory)
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .Build();


                helper.DebugLogger.LogInvocation();

                int orgId = int.Parse(req.Query["org"]);
                if ( orgId.Equals(null))
                {
                    return new NoContentResult();
                
                }

                EventController Evtctr = new EventController(config, helper);

                
                var ResponseList=Evtctr.getEventsByOrg(orgId);



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
