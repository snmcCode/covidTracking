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
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route ="ticket")] HttpRequest req,
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


                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();


                helper.DebugLogger.RequestBody = requestBody;

                helper.DebugLogger.LogRequestBody();


                Ticket data = JsonConvert.DeserializeObject<Ticket>(requestBody);


                //if (string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Hall))
                //{
                //    return new NoContentResult();
                //}



                EventController Evtctr = new EventController(config, helper);
                Ticket returTicket = Evtctr.bookTicket(data);

                return new OkObjectResult(returTicket);

            }
            catch (SqlDatabaseException e)
            {
                if (helper.DebugLogger.InnerException.Message.Contains("duplicate"))
                {
                    helper.DebugLogger.OuterException = e;
                    helper.DebugLogger.Description = "Duplicate Entry";
                    helper.DebugLogger.OuterExceptionType = "SqlDatabaseException";
                    helper.DebugLogger.Success = false;
                    helper.DebugLogger.StatusCode = CustomStatusCodes.DUPLICATE;
                    helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                    helper.DebugLogger.LogWarning();
                }
                else
                {

                    helper.DebugLogger.OuterException = e;
                    helper.DebugLogger.Description = "SqlDatabaseException";
                    helper.DebugLogger.OuterExceptionType = "SqlException";
                    helper.DebugLogger.Success = false;
                    helper.DebugLogger.StatusCode = CustomStatusCodes.SQLDATABASEERROR;
                    helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                    helper.DebugLogger.LogFailure();
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
