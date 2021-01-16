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
using System.Globalization;

using Common.Models;
using Common.Resources;
using Common.Utilities;
using Common.Utilities.Exceptions;
using common.Models;
using common.Utilities;

namespace BackEnd
{
    public  class CreateEvent
    {
        private readonly IConfiguration config;

        public CreateEvent(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("CreateEvent")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = "event")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            try
            {


                LoggerHelper helper = new LoggerHelper(log, "CreateEvent", "POST", "event");

                helper.DebugLogger.LogInvocation();
                string requestBody;

                using (var streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }

                helper.DebugLogger.RequestBody = requestBody;
                helper.DebugLogger.LogRequestBody();


                Event data = JsonConvert.DeserializeObject<Event>(requestBody);

                if (string.IsNullOrEmpty(data.Name)|| string.IsNullOrEmpty(data.Hall))
                {
                    return new NoContentResult();
                }


                EventController Evtctr = new EventController(config, helper);
                Event returnevent = await Evtctr.CreateEvent(data);

                return new OkObjectResult(data.DateTime);
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new StatusCodeResult(500);
            }
        }
    }
}





