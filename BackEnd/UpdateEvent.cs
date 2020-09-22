using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using Common.Models;
using Common.Resources;
using Common.Utilities;
using Common.Utilities.Exceptions;
using common.Models;
using common.Utilities;

namespace BackEnd
{
    public static class UpdateEvent
    {
        [FunctionName("UpdateEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"put", Route = "event")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Helper helper = new Helper(log, "UpdateEvent", "PUT", "event");
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


                Event data = JsonConvert.DeserializeObject<Event>(requestBody);


                if (string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Hall))
                {
                    return new NoContentResult();
                }



                EventController Evtctr = new EventController(config, helper);
                Event returnevent = Evtctr.UpdateEvent(data);

               

                return new OkObjectResult(returnevent);
            }



            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new StatusCodeResult(500);
            }
        }
    }
}
