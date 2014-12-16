using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DynamicRestProxy.PortableHttpClient
{
    static class HttpClientExtensions
    {
        public async static Task<dynamic> Deserialize(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
            {
                var converter = new ExpandoObjectConverter();
                if (content.StartsWith("[")) // when the result is a list we need to tell JSonConvert
                {
                    return JsonConvert.DeserializeObject<List<dynamic>>(content, converter);
                }

                return  JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            }

            return null;
        }
    }
}
