using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using BackEnd.Models;
using BackEnd.Utilities;

namespace BackEnd
{
    public static class RetrieveVisitors
    {
        [FunctionName("RetrieveVisitors")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("RetrieveVisitors Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation(requestBody);

            VisitorSearch visitorSearch = null;
            DatabaseManager databaseManager = null;
            List<Visitor> visitors = new List<Visitor>();
            string errorMessage = "";
            bool success = true;

            try
            {
                visitorSearch = JsonConvert.DeserializeObject<VisitorSearch>(requestBody);
                databaseManager = new DatabaseManager(log, config);
                visitors = databaseManager.GetVisitors(visitorSearch);
            }

            catch (JsonSerializationException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Bad Request Body";
            }

            catch (ApplicationException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Database Error";
            }

            catch (DataException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Bad Request Body";
            }

            return success
                ? (ActionResult)new OkObjectResult(visitors)
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
