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
    public class UpdateEvent
    {
        private readonly IConfiguration config;

        public UpdateEvent(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("UpdateEvent")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"put", Route = "event")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            LoggerHelper helper = new LoggerHelper(log, "UpdateEvent", "PUT", "event");
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


                Event data = JsonConvert.DeserializeObject<Event>(requestBody);


                if (string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Hall))
                {
                    return new NoContentResult();
                }



                EventController Evtctr = new EventController(config, helper);
                Event returnevent = await Evtctr.UpdateEvent(data);

               

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
