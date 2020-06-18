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
        public static async Task<Guid?> RegisterUser(Visitor visitor)
        {
             var url = $"{Utils.API_URL}/{Utils.REGISTER_FUNCTION}?code={Utils.CODE}";
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
