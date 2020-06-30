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
using BackEnd.Utilities.Models;

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
            bool success = true;
            int StatusCode = CustomStatusCodes.PLACEHOLDER;
            string ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
            ResultInformation resultInformation = null;

            try
            {
                databaseManager = new DatabaseManager(organization, log, config);
                organization = databaseManager.GetOrganization(Id);
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
                ErrorMessage = $"Organization: {CustomStatusCodes.GetStatusCodeDescription(StatusCode)}";
            }

            if (!success)
            {
                resultInformation = new ResultInformation(StatusCode, ErrorMessage);
            }

            return success
                ? (ActionResult)new OkObjectResult(organization)
                : new ObjectResult(resultInformation)
                { StatusCode = StatusCode };
        }
    }
}
