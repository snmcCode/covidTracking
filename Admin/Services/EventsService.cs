using Admin.Models;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Admin.Util;

namespace Admin.Services
{
    public class EventsService
    {
        public static async Task<List<EventModel>> GetEvents(string url, string targetResource, ILogger logger)
        {

            LoggerHelper helper = new LoggerHelper(logger, "GetEvents", null, "EventsService/GetEvents");
            helper.DebugLogger.LogInvocation();

            try
            {

                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Get, null);
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

         public static async Task<string> CreateEvent(string url, string targetResource, ILogger logger, String jsonBody)
        {

            LoggerHelper helper = new LoggerHelper(logger, "CreateEvent", null, "EventsService/CreateEvent");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post, body);
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


        public static async Task<string> UpdateEvent(string url, string targetResource, ILogger logger, String jsonBody)
        {

            LoggerHelper helper = new LoggerHelper(logger, "UpdateEvent", null, "EventsService/UpdateEvent");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Put, body);
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

        public static async Task<string> DeleteEvent(string url, string targetResource, ILogger logger, String jsonBody)
        {

            LoggerHelper helper = new LoggerHelper(logger, "DeleteEvent", null, "EventsService/DeleteEvent");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Delete, body);
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

        public static async Task<string> GroupEvents(string url, string targetResource, ILogger logger, String jsonBody)
        {

            LoggerHelper helper = new LoggerHelper(logger, "GroupEvents", null, "EventsService/GroupEvents");
            helper.DebugLogger.LogInvocation();

            var body = new StringContent(jsonBody);
            try
            {
                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Patch, body);
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

         public static async Task<List<VisitorModel>> GetUsersByEvent(string url, string targetResource, ILogger logger)
        {

            LoggerHelper helper = new LoggerHelper(logger, "GetUsersByEvent", null, "EventsService/GetUsersByEvent");
            helper.DebugLogger.LogInvocation();

            try
            {

                var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Get, null);
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
                        List<VisitorModel> visitors = JsonConvert.DeserializeObject<List<VisitorModel>>(data);
                        return visitors;
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

