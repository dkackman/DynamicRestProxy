using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    static class Extensions
    {
        public static async Task<dynamic> ExecuteDynamicGetTaskAsync(this RestClient client, RestRequest request)
        {
            var response = await client.ExecuteGetTaskAsync(request);
            return JsonConvert.DeserializeObject(response.Content);
        }

    }
}
