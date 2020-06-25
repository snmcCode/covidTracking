using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FrontEnd.Models;
using MasjidTracker.FrontEnd.Models;
using Newtonsoft.Json;

namespace FrontEnd
{
    public class UserService
    {
        public static async Task<Visitor> GetUser(string id)
        {
            var url = String.Format(Utils.RETRIEVE_USER_API_URL, id);
            using (var client = new HttpClient())
            {                           
                var result = await client.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var data = await result.Content.ReadAsStringAsync();
                    var visitor = JsonConvert.DeserializeObject<Visitor>(data);
                    return visitor;
                }

                return null;
            }
        }

        public static async Task<Visitor> GetUsers(Visitor visitor)
        {
            var url = String.Format(Utils.RETRIEVE_USERS_API_URL,visitor.FirstName, visitor.LastName, HttpUtility.UrlEncode(visitor.PhoneNumber));
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(url);

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
                return null;
            }
        }

        public static async Task<Guid?> RegisterUser(Visitor visitor)
        {
            var url = Utils.REGISTER_API_URL;
            using (var client = new HttpClient())
            {
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

        public static async Task<string> RequestCode(SMSRequestModel requestModel)
        {
            var url = Utils.REQUEST_CODE_API_URL;
            using (var client = new HttpClient())
            {
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

        public static async Task<VisitorPhoneNumberInfo> VerifyCode(SMSRequestModel requestModel)
        {
            var url = Utils.VERIFY_CODE_API_URL;
            using (var client = new HttpClient())
            {
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
