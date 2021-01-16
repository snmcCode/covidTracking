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

namespace BackEnd
{
    public  class GetEventsByOrg
    {
        private readonly IConfiguration config ;

        public GetEventsByOrg(IConfiguration config )
        {
            this.config = config;
        }

        [FunctionName("GetEventsByOrg")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",Route = "event")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            LoggerHelper helper = new LoggerHelper(log, "GetEventByOrg", "GET", "event");

            try
            {

                helper.DebugLogger.LogInvocation();

               

                var orgId = String.IsNullOrEmpty(req.Query["orgId"]) ? -1 : int.Parse(req.Query["orgId"]);
                if ( orgId.Equals(-1))
                {
                    return new NoContentResult();
                
                }

                EventController Evtctr = new EventController(config, helper);





                if (!String.IsNullOrEmpty(req.Query["startDate"]))
                {
                    var startDate = req.Query["startDate"];
                    var startDate1 = Convert.ToDateTime(startDate);
                    if (!String.IsNullOrEmpty(req.Query["endDate"]))
                    {
                        
                        var endDate = req.Query["endDate"];
                        var endDate1=Convert.ToDateTime(endDate);
                        var ResponseList1 = await Evtctr.getEventsByOrg(orgId, startDate, endDate);
                        return new OkObjectResult(ResponseList1);
                    }
                    var ResponseList2 = await Evtctr.getEventsByOrg(orgId, startDate);
                    return new OkObjectResult(ResponseList2);
                }
                if (!String.IsNullOrEmpty(req.Query["endDate"]))
                {
                    var endDate = req.Query["endDate"];
                    var endDate1 = Convert.ToDateTime(endDate);
                    var ResponseList1 =await Evtctr.getEventsByOrg(orgId,"", endDate);
                    return new OkObjectResult(ResponseList1);
                }



                var ResponseList =await Evtctr.getEventsByOrg(orgId);
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
