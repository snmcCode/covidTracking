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

using BackEnd.Models;
using BackEnd.Utilities;

namespace BackEnd
{
    public static class RegisterOrganization
    {
        [FunctionName("RegisterOrganization")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "organization")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("RegisterOrganization Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation(requestBody);

            Organization organization = null;
            string errorMessage = "";
            bool success = true;

            try
            {
                organization = JsonConvert.DeserializeObject<Organization>(requestBody);
                DatabaseManager databaseManager = new DatabaseManager(organization, log, config);
                databaseManager.AddOrganization();
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

            return success
                ? (ActionResult)new OkObjectResult(organization)
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
