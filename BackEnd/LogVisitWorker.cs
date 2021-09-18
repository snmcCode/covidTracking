using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Common.Models;
using Newtonsoft.Json;
using Common.Utilities;
using Microsoft.Extensions.Configuration;
using Common.Utilities.Exceptions;
using Common.Resources;
using System.Threading.Tasks;

namespace BackEnd
{
    public class LogVisitWorker
    {
        private readonly IConfiguration config;

        public LogVisitWorker(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("LogVisitWorker")]
        public async Task Run([QueueTrigger("%VisitsQueue%", Connection = "StorageConnectionString")] string myQueueItem, ILogger log)
        {
            LoggerHelper helper = new LoggerHelper(log, "LogVisitWorker", "QueueTrigger", "visits");
            // helper.DebugLogger.LogCustomInformation("received message " + myQueueItem);
            helper.DebugLogger.LogInvocation();
            try
            {
                Visit visit = JsonConvert.DeserializeObject<Visit>(myQueueItem);
                DatabaseManager databaseManager = new DatabaseManager(helper, config);
                visit.GenerateDateTime();
                databaseManager.SetDataParameter(visit);
                databaseManager.SetDataParameter(visit.Visitor);
                await databaseManager.LogVisit();
            }
            catch (NoSqlDatabaseException e)
            {
                helper.DebugLogger.OuterException = e;
                helper.DebugLogger.OuterExceptionType = "NoSqlDatabaseException";
                helper.DebugLogger.Success = false;
                helper.DebugLogger.StatusCode = CustomStatusCodes.NOSQLDATABASEERROR;
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

        }
    }
}
