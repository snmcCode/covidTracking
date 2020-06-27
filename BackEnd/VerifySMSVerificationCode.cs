using System;
using System.Data;
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

namespace BackEnd
{
    public static class VerifySMSVerificationCode
    {
        [FunctionName("VerifySMSVerificationCode")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/verify")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation("VerifySMSVerificationCode Invoked");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation(requestBody);

            TwilioManager twilioManager = null;
            string errorMessage = "";
            bool success = true;

            try
            {
                VisitorPhoneNumberInfo visitorPhoneNumberInfo = JsonConvert.DeserializeObject<VisitorPhoneNumberInfo>(requestBody);
                twilioManager = new TwilioManager(visitorPhoneNumberInfo, log, config);
                twilioManager.VerifyPhoneNumber();
                visitorPhoneNumberInfo = twilioManager.GetVisitorPhoneNumberInfo();

                if (visitorPhoneNumberInfo.VerificationStatus == "approved")
                {
                    Visitor visitor = new Visitor
                    {
                        Id = visitorPhoneNumberInfo.Id,
                        IsVerified = true
                    };
                    DatabaseManager databaseManager = new DatabaseManager(visitor, log, config);
                    databaseManager.UpdateVisitor();
                }

            }

            catch (JsonSerializationException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Bad Request Body";
            }

            catch (ApplicationException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Database Error";
            }

            catch (DataException e)
            {
                log.LogError(e.Message);
                success = false;
                errorMessage = "Bad Request Body";
            }

            return success
                ? (ActionResult)new OkObjectResult(twilioManager.GetVisitorPhoneNumberInfo())
                : new BadRequestObjectResult(errorMessage);
        }
    }
}
