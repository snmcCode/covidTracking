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
    public class RegisterVisitor
    {
        private readonly IConfiguration config;

        public RegisterVisitor(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("RegisterVisitor")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
         

            LoggerHelper helper = new LoggerHelper(log, "RegisterVisitor", "POST", "user");

            helper.DebugLogger.LogInvocation();

            using (var streamReader = new StreamReader(req.Body))
            {
                helper.DebugLogger.RequestBody = await streamReader.ReadToEndAsync();
            }

            helper.DebugLogger.LogRequestBody();

            DatabaseManager databaseManager = null;

            try
            {
                Visitor visitor = JsonConvert.DeserializeObject<Visitor>(helper.DebugLogger.RequestBody);
                databaseManager = new DatabaseManager(visitor, helper, config);
                await databaseManager.AddVisitor();
                helper.DebugLogger.LogSuccess();
            }
            catch (ApplicationException e)
            {
                if (e.Message == "BLOCKED_USER")
                {
                    helper.DebugLogger.OuterException = e;
                    helper.DebugLogger.Description = "Event intended for Different Audience";
                    helper.DebugLogger.OuterExceptionType = "ApplicationException";
                    helper.DebugLogger.Success = false;
                    helper.DebugLogger.StatusCode = CustomStatusCodes.BLOCKED_USER;
                    helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                    helper.DebugLogger.LogWarning();
                }
                else
                {
                    helper.DebugLogger.OuterException = e;
                    helper.DebugLogger.Description = "Event intended for Different Audience";
                    helper.DebugLogger.OuterExceptionType = "ApplicationException";
                    helper.DebugLogger.Success = false;
                    helper.DebugLogger.StatusCode = CustomStatusCodes.GENERALERROR;
                    helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                    helper.DebugLogger.LogFailure();
                }
                throw e;
            }

            catch (JsonSerializationException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "JsonSerializationException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.BADREQUESTBODY;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                throw e;
            }

            catch (SqlDatabaseException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "SqlDatabaseException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.SQLDATABASEERROR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                throw e;
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
                throw e;
            }

            return helper.DebugLogger.Success
                ? (ActionResult)new OkObjectResult(databaseManager.GetVisitorId())
                : new ConflictObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
