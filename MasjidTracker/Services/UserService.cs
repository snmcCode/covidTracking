using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using FrontEnd.Models;
using MasjidTracker.FrontEnd.Models;
using Newtonsoft.Json;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;

namespace FrontEnd
{
    public class UserService
    {
      

        public static async Task<Visitor> GetUser(string url, string targetResource, ILogger logger)
        {
            Helper helper = new Helper(logger, "GetUser", null, "UserService/GetUser");
            helper.DebugLogger.LogInvocation();
                    
                try
                {
                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Get,null);

                    if (result.IsSuccessStatusCode)
                    {
                        var data = await result.Content.ReadAsStringAsync();
                        var visitor = JsonConvert.DeserializeObject<Visitor>(data);
                        return visitor;
                    }

                }
                catch (Exception e)
                {
                    var errorMessage = e.Message;
                helper.DebugLogger.LogCustomError(errorMessage);
                }

                return null;
           
        }


    
        public static async Task<Visitor> GetUsers(string url, string targetResource, ILogger logger)
        {

            Helper helper = new Helper(logger, "GetUsers", null, "UserService/GetUsers");
            helper.DebugLogger.LogInvocation();

            try
            {

                var result = await Utils.CallAPI(url, targetResource, logger,HttpMethod.Get,null);
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;

                    helper.DebugLogger.LogWarning(reasonPhrase+ "when calling backend. url: " + url + "\n target resource: " + targetResource + " with status code " + result.StatusCode);
                } else if(result.StatusCode != HttpStatusCode.OK)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;

                    helper.DebugLogger.LogCustomError("error calling backend. url: " + url + "\n target resource: " + targetResource + " with status code " + result.StatusCode);

                }
                        if (result.IsSuccessStatusCode)
                        {
                            var data = await result.Content.ReadAsStringAsync();

                            try
                            {
                                List<Visitor> visitors = JsonConvert.DeserializeObject<List<Visitor>>(data);
                                return visitors[0];
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

        public static async Task<Guid?> RegisterUser(string url, Visitor visitor, string targetResource, ILogger logger)
        {

            Helper helper = new Helper(logger, "RegisterUser", null, "UserService/RegisterUser");
            helper.DebugLogger.LogInvocation();

            var json = JsonConvert.SerializeObject(visitor, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                var body = new StringContent(json);
            var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post,body);

                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();

                    //TODO: not sure why there's an extra set of ""
                    data = data.Replace("\"", "");

                    return new Guid(data);
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
            var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post,body);

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
            var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post,body);

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
