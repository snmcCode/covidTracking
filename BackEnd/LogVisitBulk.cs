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
using System.Collections.Generic;
using System.Text;

namespace BackEnd
{
    public class LogVisitBulk
    {
        private readonly IConfiguration config;

        public LogVisitBulk(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("LogVisitBulk")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "visits/bulk")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            LoggerHelper helper = new LoggerHelper(log, "LogVisitBulk", "POST", "visits/bulk");

            helper.DebugLogger.LogInvocation();

            using (var streamReader = new StreamReader(req.Body))
            {
                helper.DebugLogger.RequestBody = await streamReader.ReadToEndAsync();
            }
            helper.DebugLogger.LogRequestBody();

            // THIS COMMENT IS USED TO TRIGGER A NEW PR

            try
            {
                List<Visit> visitList = JsonConvert.DeserializeObject<List<Visit>>(helper.DebugLogger.RequestBody);
                for (int i = 0; i < visitList.Count; i++)
                {
                    // Set the visit offline property to be true since we know that these visits occurred while the scanner was offline -> this will bypass checking if the visitor was booked for the event or not, since the API would have let them in on scan time
                    visitList[i].Offline = true;

                    // Set the request body to be the visit
                    req.Body = new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(visitList[i])));

                    var result = await (new LogVisit(config)).Run(req, log, context);

                    // Failure occurred
                    if (result.GetType() != typeof(OkObjectResult))
                    {
                        ObjectResult typedResult = (ObjectResult)result;
                        int statusCode = (int)typedResult.StatusCode;

                        if (statusCode == CustomStatusCodes.SQLDATABASEERROR)
                        {
                            throw new SqlDatabaseException(CustomStatusCodes.GetStatusCodeDescription(statusCode));
                        }
                        else if (statusCode == CustomStatusCodes.NOSQLDATABASEERROR)
                        {
                            throw new NoSqlDatabaseException(CustomStatusCodes.GetStatusCodeDescription(statusCode));
                        }
                        else if (statusCode == CustomStatusCodes.GENERALERROR)
                        {
                            throw new Exception(CustomStatusCodes.GetStatusCodeDescription(statusCode));
                        }

                    }
                }
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

            catch (NoSqlDatabaseException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "NoSqlDatabaseException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.NOSQLDATABASEERROR;
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
                ? (ActionResult)new OkObjectResult("Success")
                : new ObjectResult(helper.DebugLogger.StatusCodeDescription)
                { StatusCode = helper.DebugLogger.StatusCode };
        }
    }
}
