using Admin.Models;
using Admin.Pages.Home;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Admin.Util;

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
    }
}
