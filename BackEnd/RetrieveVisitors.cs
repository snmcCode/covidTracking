using System;
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

using Common.Models;
using BackEnd.Utilities;
using BackEnd.Utilities.Exceptions;

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

            // Recommended to keep
            _ = await new StreamReader(req.Body).ReadToEndAsync();

            List<Visitor> visitors = new List<Visitor>();
            string errorMessage = "";
            bool success = true;

            VisitorSearch visitorSearch = new VisitorSearch
            {
                FirstName = req.Query["FirstName"],
                LastName = req.Query["LastName"],
                PhoneNumber = req.Query["PhoneNumber"],
                Email = req.Query["Email"]
            };

            try
            {
                DatabaseManager databaseManager = new DatabaseManager(log, config);
                visitors = databaseManager.GetVisitors(visitorSearch);
            }

            catch (JsonSerializationException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Bad Request Body";
            }

            catch (SqlDatabaseException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Error Occurred During Database Operation or Connection. Try Again. Contact Support if Error Persists";
            }

            catch (BadRequestBodyException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Visitor Not Found. Double Check Request Body";
            }

            return success
                ? (ActionResult)new OkObjectResult(visitors)
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
