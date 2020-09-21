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
    public static class CreateEvent
    {
        [FunctionName("CreateEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Helper helper = new Helper(log, "CreateEvent", "POST", "user");

            helper.DebugLogger.LogInvocation();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();


            helper.DebugLogger.RequestBody = requestBody;

            helper.DebugLogger.LogRequestBody();




            log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            Event data = JsonConvert.DeserializeObject<Event>(requestBody);
            EventController Evtctr = new EventController(config, helper);
            Event returnevent = Evtctr.CreateEvent(data);
            
            //var responseMessage = JsonConvert.SerializeObject(data);
            //? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //: $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(returnevent);
        }
    }
}





