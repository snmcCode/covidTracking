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
using Common.Utilities.Exceptions;
using Common.Resources;
using common.Utilities;
using common.Models;

namespace BackEnd
{
    public class UnregisterFromEvent
    {
        private readonly IConfiguration config;

        public UnregisterFromEvent(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("UnregisterFromEvent")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "event/booking")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            LoggerHelper helper = new LoggerHelper(log, "UnregisterFromEvent", "DELETE", "event/booking");

            try
            {


                helper.DebugLogger.LogInvocation();


                string requestBody;

                using (var streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }

                helper.DebugLogger.RequestBody = requestBody;

                helper.DebugLogger.LogRequestBody();

                if (requestBody.Equals(""))
                {
                    return new NoContentResult();
                }


                UnregisterRequest data = JsonConvert.DeserializeObject<UnregisterRequest>(requestBody);
                
                


                if (data.eventId==0 || data.visitorId.Equals(null) )
                {
                    return new ObjectResult("Incorrect Body Parameters") { StatusCode = 400 };
                }
                EventController Evtctr = new EventController(config, helper);
                await Evtctr.Unregister(data);

                return new OkObjectResult("Success");

            }
            catch (SqlDatabaseException e)
            {


                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.Description = "SqlDatabaseException";
                helper.DebugLogger.OuterExceptionType = "SqlException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.SQLDATABASEERROR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();

            }


            catch (Newtonsoft.Json.JsonReaderException e)
            {


                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "JsonReaderException";
                helper.DebugLogger.Description = "Input not in correct format";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.BADREQUESTBODY;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                log.LogError(e.Message);
            }
            catch (Newtonsoft.Json.JsonSerializationException e)
            {


                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "JsonSerializationException";
                helper.DebugLogger.Description = "Input metrics are not populated with values";
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
