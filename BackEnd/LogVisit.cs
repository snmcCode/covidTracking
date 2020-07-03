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
using BackEnd.Utilities;
using BackEnd.Utilities.Exceptions;

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

            log.LogInformation("LogVisit Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            bool success = true;
            int StatusCode = CustomStatusCodes.PLACEHOLDER;
            string ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);

            string recordID = null;

            try
            {
                Visit visit = JsonConvert.DeserializeObject<Visit>(requestBody);

                // Get Visitor Info
                DatabaseManager databaseManager = new DatabaseManager(log, config);
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
            }

            catch (JsonSerializationException e)
            {
                log.LogError(e.Message);
                success = false;
                StatusCode = CustomStatusCodes.BADREQUESTBODY;
                ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
            }

            catch (NoSqlDatabaseException e)
            {
                log.LogError(e.Message);
                success = false;
                StatusCode = CustomStatusCodes.NOSQLDATABASEERROR;
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
                ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
            }

            catch (UnverifiedException e)
            {
                log.LogError(e.Message);
                success = false;
                StatusCode = CustomStatusCodes.UNVERIFIEDVISITOR;
                ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
            }

            catch (BadRequestBodyException e)
            {
                log.LogError(e.Message);
                success = false;
                StatusCode = CustomStatusCodes.BADBUTVALIDREQUESTBODY;
                ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
            }

            if (recordID != null)
            {
                success = true;
            }
            else
            {
                if (StatusCode == CustomStatusCodes.PLACEHOLDER)
                {
                    // Only run this if another exception was not already thrown
                    success = false;
                    StatusCode = CustomStatusCodes.BADBUTVALIDREQUESTBODY;
                    ErrorMessage = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
                }
            }

            return success
                ? (ActionResult)new OkObjectResult(recordID)
                : new ObjectResult(ErrorMessage)
                { StatusCode = StatusCode };
        }
    }
}
