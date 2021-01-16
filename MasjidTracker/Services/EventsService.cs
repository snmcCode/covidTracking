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
using FrontEnd.Interfaces;
using FrontEnd.Services;

namespace FrontEnd.Services
{
    public class EventsService : AzureServiceBase, IEventsService
    {

        public EventsService(IHttpClientFactory httpClientFactory, ILogger<EventsService> logger) : base(httpClientFactory, logger)
        {
        }
        public async Task<List<EventModel>> GetEvents(string url, string targetResource)
        {

            LoggerHelper helper = new LoggerHelper(logger, "GetEvents", null, "EventsService/GetEvents");
            helper.DebugLogger.LogInvocation();

            try
            {

                var result = await base.CallAPI(url, targetResource, HttpMethod.Get, null);
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;

                    helper.DebugLogger.LogWarning(reasonPhrase + "when calling backend. url: " + url + "\n target resource: " + targetResource + " with status code " + result.StatusCode);
                }
                else if (result.StatusCode != HttpStatusCode.OK)
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
                        List<EventModel> events = JsonConvert.DeserializeObject<List<EventModel>>(data);
                        return events;
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

        public async Task<string> UpdateEvent(string url, string targetResource, String jsonBody)
        {

            LoggerHelper helper = new LoggerHelper(logger, "UpdateEvent", null, "EventsService/UpdateEvent");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await base.CallAPI(url, targetResource, HttpMethod.Put, body);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;

                    helper.DebugLogger.LogCustomError("error calling backend. url: " + url + "\n target resource: " + targetResource + "\n reason phrase: " + reasonPhrase + "\n message: " + message);
                }
                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();
                    return data;
                }
            }
            catch (Exception e)
            {
                helper.DebugLogger.LogCustomError(e.Message);
            }
            return null;
        }

        public async Task<int> RegisterInEvent(string url, string targetResource, String jsonBody)
        {

            LoggerHelper helper = new LoggerHelper(logger, "RegisterEvent", null, "EventsService/RegisterEvent");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await base.CallAPI(url, targetResource, HttpMethod.Post, body);

                return (int)result.StatusCode;
            }
            catch (Exception e)
            {
                helper.DebugLogger.LogCustomError(e.Message);
            }
            return 0;
        }
        public async Task<string> UnregisterFromEvent(string url, string targetResource, String jsonBody)
        {

            LoggerHelper helper = new LoggerHelper(logger, "UnregisterEvent", null, "EventsService/UnregisterEvent");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await base.CallAPI(url, targetResource, HttpMethod.Delete, body);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    var reasonPhrase = result.ReasonPhrase;
                    var message = result.RequestMessage;

                    helper.DebugLogger.LogCustomError("error calling backend. url: " + url + "\n target resource: " + targetResource + "\nMessage: " + message + "\nReasonPhrase: " + reasonPhrase);
                }
                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();
                    return data;
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

