﻿using Admin.Models;
using Admin.Pages.Home;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Admin.Util;
using System.Net.Http.Headers;

namespace Admin.Services
{
    public class UserService
    {
        public static async Task<Guid?> RegisterUser(string url, string targetResource, ILogger logger, String jsonBody)
        {

            Helper helper = new Helper(logger, "RegisterUser", null, "UserService/RegisterUser");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post, body);
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
                        Console.WriteLine("*** in userservice. result success " + data);
                    }
                    catch (Exception e)
                    {
                        helper.DebugLogger.LogCustomError(e.Message);
                    }
                    data = data.Replace("\"", "");
                    return new Guid(data);
                }
            }
            catch (Exception e)
            {
                helper.DebugLogger.LogCustomError(e.Message);
            }
            return null;
        }

        public static async Task<VisitorModel> GetOrganization(string url, string targetResource, ILogger logger, String jsonBody)
        {

            Helper helper = new Helper(logger, "GetOrganization", null, "UserService/GetOrganization");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {

                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post, body);
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
                        Console.WriteLine("*** in userservice. result success " + data);
                        // List<VisitorModel> visitors = JsonConvert.DeserializeObject<List<VisitorModel>>(data);
                        // return visitors[0];
                    }
                    catch (Exception e)
                    {
                        helper.DebugLogger.LogCustomError(e.Message);
                    }
                }


            }
            catch (Exception e)
            {
                helper.DebugLogger.LogCustomError(e.Message);
            }
            return null;
        }
    }
}
