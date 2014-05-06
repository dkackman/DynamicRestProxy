using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    static class HttpClientExtensions
    {
        public async static Task<dynamic> Deserialize(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(content));
            }

            return null;
        }        
    }
}
