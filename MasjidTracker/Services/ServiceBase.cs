using Common.Utilities;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FrontEnd.Services
{
    public abstract class AzureServiceBase
    {
        private readonly IHttpClientFactory httpClientFactory;
        protected readonly ILogger logger;


        public AzureServiceBase(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public static async Task<string> GetToken(string targetResource)
        {

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(targetResource);
            return accessToken;

        }

        protected async Task<HttpResponseMessage> CallAPI(string url, string targetResource, HttpMethod method, HttpContent body)
        {
            LoggerHelper helper = new LoggerHelper(logger, "CallAPI", null, "Utils/CallAPI");
            //skip token if localhost
            var token = "";
            if(!url.Contains("localhost"))
             {
                token = await GetToken(targetResource);
            }
            
            var client = httpClientFactory.CreateClient();

            var httpMessage = new HttpRequestMessage();


            httpMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpMessage.Method = method;
            httpMessage.RequestUri = new Uri(url);
            if (method == HttpMethod.Post || method == HttpMethod.Delete || method == HttpMethod.Put)
            {
                httpMessage.Content = body;
            }

            var result = await client.SendAsync(httpMessage);

            if (result.IsSuccessStatusCode)
            {
                return result;
            }
            else
            {
                var reasonPhrase = result.ReasonPhrase;
                var message = result.RequestMessage;
                var logMessage = reasonPhrase + "\n" + message;
                helper.DebugLogger.LogCustomCritical(logMessage);
                return result;
            }

        }
    }

}

