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
using BackEnd.Utilities;
using BackEnd.Utilities.Exceptions;

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

                // Get Visitor Info
                DatabaseManager databaseManager = new DatabaseManager(log, config);
                Visitor visitor = databaseManager.GetVisitor(visit.VisitorId);

                // Set parameters on Visit
                visit.Visitor = visitor;
                visit.GenerateDateTime(); // This should only be called if the Date and Time are not being sent by the Front-End
                visit.GenerateId();

                // Set Visit on DatabaseManager
                databaseManager.SetDataParameter(visit);

                // LogVisit
                recordID = await databaseManager.LogVisit();
            }

            catch (JsonSerializationException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Bad Request Body";
            }

            catch (NoSqlDatabaseException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Error Occurred During Database Operation or Connection. Try Again. Contact Support if Error Persists";
            }

            catch (SqlDatabaseException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Error Occurred During Database Operation or Connection. Try Again. Contact Support if Error Persists";
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
