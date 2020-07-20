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

namespace FrontEnd
{
    public class UserService
    {        
        public static async Task<string> GetToken()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("api://56729bec-fe4f-4480-a6f4-fb9fe969d5fe");
            return accessToken;
        }

        public static async Task<Visitor> GetUser(string url)
        {
            var token = await GetToken();
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

        public static async Task<Visitor> GetUsers(string url)
        {
            var token = await GetToken();
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

        public static async Task<Guid?> RegisterUser(string url, Visitor visitor)
        {
            var token = await GetToken();
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

        public static async Task<string> RequestCode(string url, SMSRequestModel requestModel)
        {
            var token = await GetToken();
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

        public static async Task<VisitorPhoneNumberInfo> VerifyCode(string url, SMSRequestModel requestModel)
        {
            var token = await GetToken();
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
