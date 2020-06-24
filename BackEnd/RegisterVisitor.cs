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

namespace BackEnd
{
    public static class RegisterVisitor
    {
        [FunctionName("RegisterVisitor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("RegisterVisitor Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation(requestBody);

            DatabaseManager databaseManager = null;
            string errorMessage = "";
            bool success = true;

            try
            {
                Visitor visitor = JsonConvert.DeserializeObject<Visitor>(requestBody);
                databaseManager = new DatabaseManager(visitor, log, config);
                databaseManager.AddVisitor();
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
                ? (ActionResult)new OkObjectResult(databaseManager.GetVisitorId())
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
