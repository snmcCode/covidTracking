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
using MobileAuthenticate.Utilities;
using MobileAuthenticate.Utilities.Exceptions;

namespace MobileAuthenticate
{
    public static class MobileAuthenticate
    {
        [FunctionName("MobileAuthenticate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "authenticate")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("UpdateVisitor Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation(requestBody);
            Organization organization = null;
            bool success = true;
            int StatusCode = CustomStatusCodes.PLACEHOLDER;
            string ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);

            try
            {
                ScannerLogin scannerLogin = JsonConvert.DeserializeObject<ScannerLogin>(requestBody);
                DatabaseManager databaseManager = new DatabaseManager(scannerLogin, log, config);
                organization = databaseManager.LoginScanner();
            }

            catch (BadRequestBodyException e)
            {
                log.LogError(e.Message);
                success = false;
                StatusCode = CustomStatusCodes.BADBUTVALIDREQUESTBODY;
                ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
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

            return success
                ? (ActionResult)new OkObjectResult(organization)
                : new ObjectResult(ErrorMessage)
                { StatusCode = StatusCode };
        }
    }
}
