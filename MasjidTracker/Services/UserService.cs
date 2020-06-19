using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
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
    }
}
