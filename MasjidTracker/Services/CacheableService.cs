﻿using Common.Models;
using Common.Utilities;
using FrontEnd.Interfaces;
using MasjidTracker.FrontEnd.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FrontEnd.Services
{
    public class CacheableService : ICacheableService
    {
        private readonly IMemoryCache cache;

        public CacheableService(IMemoryCache memoryCache)
        {
            cache = memoryCache;
        }

        public async Task<string> GetSetting(string url, string domain, string key, string targetResource, Setting mysetting, ILogger<HomeController> logger)
        {
            var cacheKey = $"{domain}:{key}";
            if (!cache.TryGetValue(cacheKey, out string value))
            {
                Helper helper = new Helper(logger, "getSetting", null, "UserService/getSetting");
                helper.DebugLogger.LogInvocation();
                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Get, null);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;

                    helper.DebugLogger.LogCustomError("error calling backend. url: " + url + "\n target resource: " + targetResource);
                }
                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();

                    try
                    {
                        mysetting = JsonConvert.DeserializeObject<Common.Models.Setting>(data);
                        value = mysetting?.value;
                        cache.Set(cacheKey, value);
                    }
                    catch (Exception e)
                    {
                        helper.DebugLogger.LogCustomError(e.Message);

                    }

                }
                return null;
            }
            return value;
           
        }
    }
}
