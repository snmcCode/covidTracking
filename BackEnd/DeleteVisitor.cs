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
using Common.Resources;
using BackEnd.Utilities;
using BackEnd.Utilities.Exceptions;

namespace BackEnd
{
    public static class DeleteVisitor
    {
        [FunctionName("DeleteVisitor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "user/{Id}")] HttpRequest req,
            string Id,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("DeleteVisitor Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation("Received requestBody");

            log.LogInformation(requestBody);

            Visitor visitor = new Visitor();
            DatabaseManager databaseManager;
            bool success = true;
            int StatusCode = CustomStatusCodes.PLACEHOLDER;
            string ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);

            try
            {
                databaseManager = new DatabaseManager(visitor, log, config);
                databaseManager.DeleteVisitor(Guid.Parse(Id));
            }

            catch (SqlDatabaseException e)
            {
                log.LogError(e.Message);
                success = false;
                StatusCode = CustomStatusCodes.SQLDATABASEERROR;
                ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
            }

            catch (SqlDatabaseDataNotFoundException e)
            {
                log.LogError(e.Message);
                success = false;
                StatusCode = CustomStatusCodes.NOTFOUNDINSQLDATABASE;
                ErrorMessage = $"Visitor: {CustomStatusCodes.GetStatusCodeDescription(StatusCode)}";
            }

            return success
                ? (ActionResult)new OkObjectResult(Id)
                : new ObjectResult(ErrorMessage)
                { StatusCode = StatusCode };
        }
    }
}
