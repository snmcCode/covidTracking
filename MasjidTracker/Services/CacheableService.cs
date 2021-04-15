﻿using Common.Models;
using Common.Utilities;
using FrontEnd.Interfaces;
using MasjidTracker.FrontEnd.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FrontEnd.Services
{
    public class CacheableService : AzureServiceBase, ICacheableService
    {
        private readonly IMemoryCache cache;

        public CacheableService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, ILogger<CacheableService> logger) : base(httpClientFactory, logger)
        {
            cache = memoryCache;

        }

        public async Task<string> GetSetting(string url, string domain, string key, string targetResource, Setting mysetting)
        {
            var cacheKey = $"{domain}:{key}";
            if (!cache.TryGetValue(cacheKey, out string value))
            {
                LoggerHelper helper = new LoggerHelper(logger, "getSetting", null, "UserService/getSetting");
                helper.DebugLogger.LogInvocation();
                var result = await base.CallAPI(url, targetResource, HttpMethod.Get, null);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;
                    helper.DebugLogger.LogCustomError("error calling backend. url: " + url + "\n target resource: " + targetResource);
                    //add empty string to the cache to reduce the traffic to backend
                    value = string.Empty;
                    cache.Set(cacheKey, value, TimeSpan.FromHours(1));
                    return null;
                }
                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();
                    try
                    {
                        mysetting = JsonConvert.DeserializeObject<Common.Models.Setting>(data);
                        value = mysetting?.value;
                        //add one hour expiration for the cache
                        cache.Set(cacheKey, value, TimeSpan.FromHours(1));
                    }
                    catch (Exception e)
                    {
                        helper.DebugLogger.LogCustomError(e.Message);

                    }
                }
            }
            return value;

        }

        public async Task<List<Organization>> GetOrgs(string url, string targetResource)
        {
            var cacheKey = "organizations";
            var orgs = new List<Organization>();
            if (!cache.TryGetValue(cacheKey, out string value))
            {
                LoggerHelper helper = new LoggerHelper(logger, "getOrgs", null, "CacheableService/getOrgs");
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
                        orgs = JsonConvert.DeserializeObject<List<Common.Models.Organization>>(data);

                        //add one hour expiration for the cache
                        cache.Set(cacheKey, data, DateTimeOffset.Now.AddHours(24));

                        return orgs;
                    }
                    catch (Exception e)
                    {
                        helper.DebugLogger.LogCustomError(e.Message);

                    }
                }
            }

            orgs = JsonConvert.DeserializeObject<List<Common.Models.Organization>>(value);
            return orgs;
        }

        public async Task<List<StatusModel>> GetStatuses(string url, string targetResource)
        {
            var cacheKey = "statuses";
            var statuses = new List<StatusModel>();
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
                        statuses = JsonConvert.DeserializeObject<List<StatusModel>>(data);

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

            statuses = JsonConvert.DeserializeObject<List<StatusModel>>(value);
            return statuses;
        }

    }
}
