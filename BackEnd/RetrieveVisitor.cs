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
using Common.Utilities;
using Common.Utilities.Exceptions;

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

            Helper helper = new Helper(log, "RetrieveVisitor", "GET", $"user/{Id}");

            helper.DebugLogger.LogInvocation();

            helper.DebugLogger.RequestBody = await new StreamReader(req.Body).ReadToEndAsync();

            helper.DebugLogger.LogRequestBody();

            Visitor visitor = null;
            DatabaseManager databaseManager;

            try
            {
                databaseManager = new DatabaseManager(visitor, helper, config);
                visitor = databaseManager.GetVisitor(Guid.Parse(Id));
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
                helper.DebugLogger.Description = "Visitor Not Found";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.NOTFOUNDINSQLDATABASE;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
            }

            return helper.DebugLogger.Success
                ? (ActionResult)new OkObjectResult(visitor)
                : new ObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
