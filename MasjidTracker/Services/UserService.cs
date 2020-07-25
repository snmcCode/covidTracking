using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using FrontEnd.Models;
using MasjidTracker.FrontEnd.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Newtonsoft.Json;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FrontEnd
{
    public class UserService
    {        
        public static async Task<string> GetToken(string targetResource)
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(targetResource);
                return accessToken;
            }
            catch (Exception)
            {

                throw;
            }
         
        }

        public static async Task<Visitor> GetUser(string url,string targetResource,ILogger logger)
        {
            Helper helper = new Helper(logger, "GetUser", null, "UserService/GetUser");
            helper.DebugLogger.LogInvocation();
            try
            {
                var token = await GetToken(targetResource);
            }
            catch (Exception e)
            {
                helper.DebugLogger.LogCustomError(e.Message);
            }
            helper.DebugLogger.LogCustomDebug(string.Format("token is {0} ",token));
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var result = await client.GetAsync(url);
                    
                    if (result.IsSuccessStatusCode)
                    {
                        var data = await result.Content.ReadAsStringAsync();
                        var visitor = JsonConvert.DeserializeObject<Visitor>(data);
                        return visitor;
                    }

                } catch (Exception e)
                {
                    var errorMessage = e.Message;
                    Console.WriteLine(errorMessage);
                }

                return null;
            }
        }

        public static async Task<Visitor> GetUsers(string url,string targetResource)
        {
            var token = await GetToken(targetResource);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var result = await client.GetAsync(url);

                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;

                    Console.Write(reasonPhrase + "\n" + message);

                    if (result.IsSuccessStatusCode)
                    {
                        var data = await result.Content.ReadAsStringAsync();

                        try
                        {
                            List<Visitor> visitors = JsonConvert.DeserializeObject<List<Visitor>>(data);
                            return visitors[0];
                        } catch (Exception e)
                        {

                        }
                    }
                }
                catch (Exception e)
                {
                    var errorMessage = e.Message;
                    Console.WriteLine(errorMessage);
                }
                return null;
            }
        }

        public static async Task<Guid?> RegisterUser(string url, Visitor visitor,string targetResource)
        {
            var token = await GetToken(targetResource);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(visitor, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                var body = new StringContent(json);
                var result = await client.PostAsync(url, body);

                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();

                    //TODO: not sure why there's an extra set of ""
                    data = data.Replace("\"", "");

                    return new Guid(data);
                }

                return null;
            }
        }

        public static async Task<string> RequestCode(string url, SMSRequestModel requestModel,string targetResource)
        {
            var token = await GetToken(targetResource);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(requestModel, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                var body = new StringContent(json);
                var result = await client.PostAsync(url, body);

                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();
   
                    return data.ToString();
                }

                return null;
            }
        }

        public static async Task<VisitorPhoneNumberInfo> VerifyCode(string url, SMSRequestModel requestModel,string targetResource)
        {
            var token = await GetToken(targetResource);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(requestModel, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                var body = new StringContent(json);
                var result = await client.PostAsync(url, body);

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
}
