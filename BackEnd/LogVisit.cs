using System;
using System.Data;
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
using BackEnd.Utilities;

namespace BackEnd
{
    public static class LogVisit
    {
        [FunctionName("LogVisit")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "visits")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("LogVisit Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            string errorMessage = "";
            bool success;

            string recordID = null;

            try
            {
                Visit visit = JsonConvert.DeserializeObject<Visit>(requestBody);

                // This should only be called if the Date and Time are not being sent by the Front-End
                visit.GenerateDateTime();

                DatabaseManager databaseManager = new DatabaseManager(visit, log, config);
                recordID = await databaseManager.LogVisit();
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

            if (recordID != null)
            {
                success = true;
            }
            else
            {
                success = false;
                errorMessage = "Bad Request Body";
            }

            return success
                ? (ActionResult)new OkObjectResult(recordID)
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
