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

            log.LogInformation("SendSMSVerificationCode Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation(requestBody);

            TwilioManager twilioManager = null;
            string errorMessage = "";
            bool success = true;

            try
            {
                VisitorPhoneNumberInfo visitorPhoneNumberInfo = JsonConvert.DeserializeObject<VisitorPhoneNumberInfo>(requestBody);
                twilioManager = new TwilioManager(visitorPhoneNumberInfo, log, config);
                twilioManager.SendSMS();
            }

            catch (JsonSerializationException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Bad Request Body";
            }

            catch (TwilioAPIException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Error Occurred During Twilio API Operation or Connection. Please Double Check Request Body and Try Again. Contact Support if Error Persists";
            }

            catch (BadRequestBodyException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Could Not Find Information in Request Body. Double Check Request Body and Try Again";
            }

            return success
                ? (ActionResult)new OkObjectResult(twilioManager.GetVisitorPhoneNumberInfo())
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
