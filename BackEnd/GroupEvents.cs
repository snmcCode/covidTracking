using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Common.Utilities;
using common.Models;
using common.Utilities;
using Common.Utilities.Exceptions;
using Common.Resources;
using System.Collections.Specialized;
using System.Web;
using System.Collections.Generic;
using System.Net.Http;

namespace BackEnd
{
    public static class GroupEvents
    {
        [FunctionName("GroupEvents")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "event")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            Helper helper = new Helper(log, "GroupEvents", "PATCH", "event");

            try
            {

                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();


                helper.DebugLogger.LogInvocation();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();


                helper.DebugLogger.RequestBody = requestBody;

                helper.DebugLogger.LogRequestBody();




                var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(requestBody);



                foreach (string key in dict.Keys)
                {
                    if (!key.Contains("id") && !key.Contains("Id") && !key.Contains("ID"))
                    {
                        return new ObjectResult("Incorrect parameters recieved")
                        { StatusCode = 400 };
                    }
                }



                List<int> ids = new List<int>();




                foreach (KeyValuePair<string, int> pair in dict)
                {
                    ids.Add(pair.Value);
                }

                if (ids.Count <= 1)
                {
                    return new ObjectResult("one or less Id was recieved")
                    { StatusCode = 400 };
                }





                EventController Evtctr = new EventController(config, helper);
                await Evtctr.groupEvents(ids);



                return new OkObjectResult("Success");


            }
            catch (SqlDatabaseException e)
            {


                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.Description = "SqlDatabaseException";
                helper.DebugLogger.OuterExceptionType = "SqlException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.SQLDATABASEERROR;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();

            }


            catch (Newtonsoft.Json.JsonReaderException e)
            {


                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "JsonReaderException";
                helper.DebugLogger.Description = "Input not in correct format";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.BADREQUESTBODY;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                log.LogError(e.Message);
            }
            catch (Newtonsoft.Json.JsonSerializationException e)
            {


                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "JsonSerializationException";
                helper.DebugLogger.Description = "Input metrics are not populated with values";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.BADREQUESTBODY;
                helper.DebugLogger.StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(helper.DebugLogger.StatusCode);
                helper.DebugLogger.LogFailure();
                log.LogError(e.Message);
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


            return new ObjectResult(helper.DebugLogger.StatusCodeDescription)
            { StatusCode = helper.DebugLogger.StatusCode };


        }
    }
}


