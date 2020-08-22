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

        public static async Task<OrganizationModel> GetOrganization(string url, string targetResource, ILogger logger, OrgLoginModel orgLogin)
        {

            Helper helper = new Helper(logger, "GetOrganization", null, "UserService/GetOrganization");
            helper.DebugLogger.LogInvocation();


            var json = JsonConvert.SerializeObject(orgLogin, Newtonsoft.Json.Formatting.None,
                   new JsonSerializerSettings
                   {
                       NullValueHandling = NullValueHandling.Ignore
                   });

            var body = new StringContent(json);
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
                        return JsonConvert.DeserializeObject<OrganizationModel>(data);
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

        public static async Task<string> RequestCode(string url, SMSRequestModel requestModel, string targetResource, ILogger logger)
        {
            Helper helper = new Helper(logger, "RequestCode", null, "UserService/RequestCode");
            helper.DebugLogger.LogInvocation();

            var json = JsonConvert.SerializeObject(requestModel, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

            var body = new StringContent(json);
            var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post, body);
            Console.WriteLine("*** USER SERVICE, RESULT STATUS CODE: " + result.StatusCode);
            if (result.IsSuccessStatusCode)
            {
                var data = await result.Content.ReadAsStringAsync();

                return data.ToString();
            }

            return null;

        }

        public static async Task<VisitorPhoneNumberInfo> VerifyCode(string url, SMSRequestModel requestModel, string targetResource, ILogger logger)
        {
            Helper helper = new Helper(logger, "VerifyCode", null, "UserService/VerifyCode");
            helper.DebugLogger.LogInvocation();

            var json = JsonConvert.SerializeObject(requestModel, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

            var body = new StringContent(json);
            var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post, body);

            if (result.IsSuccessStatusCode)
            {
                var data = await result.Content.ReadAsStringAsync();
                var resultInfo = JsonConvert.DeserializeObject<VisitorPhoneNumberInfo>(data);
                return resultInfo;
            }

            return null;

        }

    }
}
