using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

using Common.Models;
using Common.Resources;
using Common.Utilities;
using Common.Utilities.Exceptions;

namespace BackEnd
{
    public static class RetrieveOrganizationDoors
    {
        [FunctionName("RetrieveOrganizationDoors")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "organization/{Id}/doors")] HttpRequest req,
            int Id,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Helper helper = new Helper(log, "RetrieveOrganizationDoors", "GET", $"organization/{Id}/doors");

            helper.DebugLogger.LogInvocation();

            helper.DebugLogger.RequestBody = await new StreamReader(req.Body).ReadToEndAsync();

            helper.DebugLogger.LogRequestBody();

            List<OrganizationDoor> organizationDoors = new List<OrganizationDoor>();

            try
            {
                DatabaseManager databaseManager = new DatabaseManager(helper, config);
                organizationDoors = databaseManager.GetOrganizationDoors(Id);
                helper.DebugLogger.LogSuccess();
            }

            catch (SqlDatabaseException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "SqlDatabaseException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.SQLDATABASEERROR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
            }

            catch (SqlDatabaseDataNotFoundException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "SqlDatabaseDataNotFoundException";
                helper.DebugLogger.Description = "Organization Not Found";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.NOTFOUNDINSQLDATABASE;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
            }

            catch (Exception e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "Exception";
                helper.DebugLogger.Description = "Generic Exception";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.GENERALERROR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                log.LogError(e.Message);
            }

            return helper.DebugLogger.Success
                ? (ActionResult)new OkObjectResult(organizationDoors)
                : new ObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
