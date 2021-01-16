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
    public class VerifySMSVerificationCode
    {
        private readonly IConfiguration config;

        public VerifySMSVerificationCode(IConfiguration config)
        {
            this.config = config;
        }
        [FunctionName("VerifySMSVerificationCode")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/verify")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
           

            LoggerHelper helper = new LoggerHelper(log, "VerifySMSVerificationCode", "POST", "user/verify");

            helper.DebugLogger.LogInvocation();

            using (var streamReader = new StreamReader(req.Body))
            {
                helper.DebugLogger.RequestBody = await streamReader.ReadToEndAsync();
            }
            helper.DebugLogger.LogRequestBody();

            TwilioManager twilioManager = null;

            try
            {
                VisitorPhoneNumberInfo visitorPhoneNumberInfo = JsonConvert.DeserializeObject<VisitorPhoneNumberInfo>(helper.DebugLogger.RequestBody);
                twilioManager = new TwilioManager(visitorPhoneNumberInfo, helper, config);
                twilioManager.VerifyPhoneNumber();
                visitorPhoneNumberInfo = twilioManager.GetVisitorPhoneNumberInfo();

                if (visitorPhoneNumberInfo.VerificationStatus == "approved")
                {
                    Visitor visitor = new Visitor
                    {
                        Id = visitorPhoneNumberInfo.Id,
                        IsVerified = true
                    };
                    DatabaseManager databaseManager = new DatabaseManager(visitor, helper, config);
                    await databaseManager.UpdateVisitor();
                }

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

            catch (TwilioApiException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "TwilioApiException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.TWILIOERROR;
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

            return helper.DebugLogger.Success
                ? (ActionResult)new OkObjectResult(twilioManager.GetVisitorPhoneNumberInfo())
                : new ObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
