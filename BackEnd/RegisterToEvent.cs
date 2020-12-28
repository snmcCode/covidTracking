using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Common.Utilities;
using common.Models;
using common.Utilities;
using Common.Utilities.Exceptions;
using Common.Resources;
namespace BackEnd
{
    public static class RegisterToEvent
    {
        [FunctionName("RegisterToEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route ="event/booking")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Helper helper = new Helper(log, "RegisterToEvent", "POST", "event/booking");

            try
            {

                IConfigurationRoot config = new ConfigurationBuilder()
                   .SetBasePath(context.FunctionAppDirectory)
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .Build();





                helper.DebugLogger.LogInvocation();


                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();


                helper.DebugLogger.RequestBody = requestBody;

                helper.DebugLogger.LogRequestBody();


                Ticket data = JsonConvert.DeserializeObject<Ticket>(requestBody);

                EventController Evtctr = new EventController(config, helper);
                Ticket returTicket = await Evtctr.bookTicket(data);

                return new OkObjectResult(returTicket);

            }
            catch (ApplicationException e)
            {
               if(e.Message== "BOOKED_SAME_GROUP")
                {
                    helper.DebugLogger.OuterException = e;
                    helper.DebugLogger.Description = "User already booked in the same event group";
                    helper.DebugLogger.OuterExceptionType = "ApplicationException";
                    helper.DebugLogger.Success = false;
                    helper.DebugLogger.StatusCode = CustomStatusCodes.BOOKED_SAME_GROUP;
                    helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                    helper.DebugLogger.LogWarning();
                }
               else if(e.Message== "EVENT_FULL")
                {
                    helper.DebugLogger.OuterException = e;
                    helper.DebugLogger.Description = "Event exceeded capacity";
                    helper.DebugLogger.OuterExceptionType = "ApplicationException";
                    helper.DebugLogger.Success = false;
                    helper.DebugLogger.StatusCode = CustomStatusCodes.EVENT_FULL;
                    helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                    helper.DebugLogger.LogWarning();
                }
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
