using Admin.Models;
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
        public static async Task<Guid?> RegisterUser(string url, VisitorModel visitor, string targetResource, ILogger<RegistrationModel> logger)
        {

            Helper helper = new Helper(logger, "RegisterUser", null, "UserService/RegisterUser");
            helper.DebugLogger.LogInvocation();

            var json = JsonConvert.SerializeObject(visitor, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

            var body = new StringContent(json);
            var result = await Utils.CallAPI(url, targetResource, logger, HttpMethod.Post, body);

            if (result.IsSuccessStatusCode)
            {
                var data = await result.Content.ReadAsStringAsync();

                //TODO: not sure why there's an extra set of ""
                data = data.Replace("\"", "");

                return new Guid(data);
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
                        Console.WriteLine("*** in userservice. result success " + data);
                        // return data;
                        return JsonConvert.DeserializeObject<OrganizationModel>(data);
                        // return orgs[0];
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
