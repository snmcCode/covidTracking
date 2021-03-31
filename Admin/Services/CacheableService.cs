using common.Models;
using Common.Utilities;
using Admin.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Admin.Services
{
     public class CacheableService : AzureServiceBase, ICacheableService
    {
        private readonly IMemoryCache cache;

        public CacheableService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, ILogger<CacheableService> logger) : base(httpClientFactory, logger)
        {
            cache = memoryCache;

        }

        public async Task<List<StatusInfo>> GetStatuses(string url, string targetResource)
        {
            var cacheKey = "statuses";
            var statuses = new List<StatusInfo>();
            if (!cache.TryGetValue(cacheKey, out string value))
            {
                LoggerHelper helper = new LoggerHelper(logger, "getStatuses", null, "CacheableService/getStatuses");
                helper.DebugLogger.LogInvocation();
                var result = await base.CallAPI(url, targetResource, HttpMethod.Get, null);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;
                    helper.DebugLogger.LogCustomError("error calling backend. url: " + url + "\n target resource: " + targetResource);
                    return null;
                }
                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();
                    try
                    {
                        statuses = JsonConvert.DeserializeObject<List<StatusInfo>>(data);

                        //add one hour expiration for the cache
                        cache.Set(cacheKey, data, DateTimeOffset.Now.AddHours(24));

                        return statuses;
                    }
                    catch (Exception e)
                    {
                        helper.DebugLogger.LogCustomError(e.Message);

                    }
                }
            }

            statuses = JsonConvert.DeserializeObject<List<StatusInfo>>(value);
            return statuses;
        }
    }
}