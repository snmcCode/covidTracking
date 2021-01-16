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

namespace BackEnd
{
    public  class GetEventsByOrgToday
    {

        private readonly IConfiguration config;

        public GetEventsByOrgToday(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("GetEventsByOrgToday")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/today")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            LoggerHelper helper = new LoggerHelper(log, "GetEventsByOrgToday", "GET", "event/today");

            try
            {


                helper.DebugLogger.LogInvocation();

                int orgId = int.Parse(req.Query["orgId"]);
                if (orgId.Equals(null))
                {
                    return new NoContentResult();

                }

                EventController Evtctr = new EventController(config, helper);


                var ResponseList =await Evtctr.getEventsByOrgToday(orgId);



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
