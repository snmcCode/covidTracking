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
    public class UpdateOrganizationCredentials
    {
        private readonly IConfiguration config;

        public UpdateOrganizationCredentials(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("UpdateOrganizationCredentials")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "organization/credentials")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
           

            LoggerHelper helper = new LoggerHelper(log, "UpdateOrganizationCredentials", "POST", "organization/credentials");

            helper.DebugLogger.LogInvocation();

            using (var streamReader = new StreamReader(req.Body))
            {
                helper.DebugLogger.RequestBody = await streamReader.ReadToEndAsync();
            }
            helper.DebugLogger.LogRequestBody();

            OrganizationCredentialInfo organizationCredentialInfo = null;

            try
            {
                organizationCredentialInfo = JsonConvert.DeserializeObject<OrganizationCredentialInfo>(helper.DebugLogger.RequestBody);
                DatabaseManager databaseManager = new DatabaseManager(helper, config);
                await databaseManager.UpdateOrganizationCredentials(organizationCredentialInfo);
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
                ? (ActionResult)new OkObjectResult(organizationCredentialInfo.Id)
                : new ConflictObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
