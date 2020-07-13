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
    public static class SendSMSVerificationCode
    {
        [FunctionName("SendSMSVerificationCode")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/sms")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Helper helper = new Helper(log, "SendSMSVerificationCode", "POST", "user/sms");

            helper.DebugLogger.LogInvocation();

            helper.DebugLogger.RequestBody = await new StreamReader(req.Body).ReadToEndAsync();

            helper.DebugLogger.LogRequestBody();

            TwilioManager twilioManager = null;

            try
            {
                VisitorPhoneNumberInfo visitorPhoneNumberInfo = JsonConvert.DeserializeObject<VisitorPhoneNumberInfo>(helper.DebugLogger.RequestBody);
                twilioManager = new TwilioManager(visitorPhoneNumberInfo, helper, config);
                twilioManager.SendSMS();
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

            return helper.DebugLogger.Success
                ? (ActionResult)new OkObjectResult(twilioManager.GetVisitorPhoneNumberInfo())
                : new ObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
