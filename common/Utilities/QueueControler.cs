using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Common.Models;
using Common.Utilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace common.Utilities
{
    public class QueueControler
    {
        private LoggerHelper Helper;
        private readonly IConfiguration Config;



        public QueueControler(LoggerHelper helper, IConfiguration config)
        {
            Helper = helper;
            Config = config;
        }
        public async Task<string> InsertMessageAsync( Visit visit)
        {
            var visitBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(visit));
            var message = System.Convert.ToBase64String(visitBytes);
            QueueClient queue = new QueueClient(Config.GetConnectionString("StorageConnectionString"), Config.GetValue<string>("VisitsQueue"));
            if (null != await queue.CreateIfNotExistsAsync())
            {
                Helper.DebugLogger.LogCustomInformation("visits-queue was created.");
              
            }

            var  result = await queue.SendMessageAsync(message);
            return result.Value.MessageId; 
        }
    }
}
