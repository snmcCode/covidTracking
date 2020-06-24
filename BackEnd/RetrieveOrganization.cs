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

using Common.Models;
using BackEnd.Utilities;

namespace BackEnd
{
    public static class RetrieveOrganization
    {
        [FunctionName("RetrieveOrganization")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "organization/{Id}")] HttpRequest req,
            int Id,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("RetrieveOrganization Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation("Received requestBody");

            log.LogInformation(requestBody);

            Organization organization = null;
            DatabaseManager databaseManager;
            string errorMessage = "";
            bool success = true;

            try
            {
                databaseManager = new DatabaseManager(organization, log, config);
                organization = databaseManager.GetOrganization(Id);
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
                errorMessage = "Organization Not Found";
            }

            return success
                ? (ActionResult)new OkObjectResult(organization)
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
