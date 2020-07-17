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
using Common.Utilities;
using Common.Utilities.Exceptions;

namespace BackEnd
{
    public static class LogVisit
    {
        [FunctionName("LogVisit")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "visits")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Helper helper = new Helper(log, "LogVisit", "POST", "visits");

            helper.DebugLogger.LogInvocation();

            helper.DebugLogger.RequestBody = await new StreamReader(req.Body).ReadToEndAsync();

            helper.DebugLogger.LogRequestBody();;

            string recordID = null;

            try
            {
                Visit visit = JsonConvert.DeserializeObject<Visit>(helper.DebugLogger.RequestBody);

                // Get Visitor Info
                DatabaseManager databaseManager = new DatabaseManager(helper, config);
                Visitor visitor = databaseManager.GetVisitor(visit.VisitorId); // Sets Visitor Property of Database Manager
                log.LogInformation($"Visitor From DB: {JsonConvert.SerializeObject(visitor)}");

                // Set parameters on Visit
                visit.Visitor = visitor;
                visit.GenerateDateTime(); // This should only be called if the Date and Time are not being sent by the Front-End
                visit.GenerateId();

                // Set Visit on DatabaseManager
                databaseManager.SetDataParameter(visit);

                // LogVisit
                recordID = await databaseManager.LogVisit();
                helper.DebugLogger.LogSuccess();
            }

            catch (JsonSerializationException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "JsonSerializationException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.BADREQUESTBODY;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
            }

            catch (NoSqlDatabaseException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "NoSqlDatabaseException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.NOSQLDATABASEERROR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
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

            catch (UnverifiedException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "UnverifiedException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.UNVERIFIEDVISITOR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
            }

            catch (BadRequestBodyException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "BadRequestBodyException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.BADBUTVALIDREQUESTBODY;
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

            if (recordID != null)
            {
                helper.DebugLogger.Success = true;
            }
            else
            {
                if (helper.DebugLogger.StatusCode == CustomStatusCodes.PLACEHOLDER)
                {
                    // Only run this if another exception was not already thrown
                    helper.DebugLogger.Success = false;
                    helper.DebugLogger.StatusCode = CustomStatusCodes.BADBUTVALIDREQUESTBODY;
                    helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                }
            }

            return helper.DebugLogger.Success
                ? (ActionResult)new OkObjectResult(recordID)
                : new ObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
