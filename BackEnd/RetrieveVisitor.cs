using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Common.Models;
using BackEnd.Utilities;
using BackEnd.Utilities.Exceptions;

namespace BackEnd
{
    public static class RetrieveVisitor
    {
        [FunctionName("RetrieveVisitor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/{Id}")] HttpRequest req,
            string Id,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("RetrieveVisitor Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation("Received requestBody");

            log.LogInformation(requestBody);

            Visitor visitor = null;
            DatabaseManager databaseManager;
            string errorMessage = "";
            bool success = true;

            try
            {
                databaseManager = new DatabaseManager(visitor, log, config);
                visitor = databaseManager.GetVisitor(Guid.Parse(Id));
            }

            catch (SqlDatabaseException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Error Occurred During Database Operation or Connection. Try Again. Contact Support if Error Persists";
            }

            catch (SqlDatabaseDataException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Visitor Not Found. Double Check Request Body";
            }

            return success
                ? (ActionResult)new OkObjectResult(visitor)
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
