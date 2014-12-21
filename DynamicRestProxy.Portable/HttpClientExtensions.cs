using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DynamicRestProxy.PortableHttpClient
{
    static class HttpClientExtensions
    {
        public async static Task<T> Deserialize<T>(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
            {
                if (typeof(T) == typeof(object))
                {
                    return Deserialize(content);
                }

                return JsonConvert.DeserializeObject<T>(content);
            }

            return default(T);
        }

        static dynamic Deserialize(string content)
        {
            Debug.Assert(!string.IsNullOrEmpty(content));

            var converter = new ExpandoObjectConverter();
            if (content.StartsWith("[")) // when the result is a list we need to tell JSonConvert
            {
                return JsonConvert.DeserializeObject<List<dynamic>>(content);
            }

            return JsonConvert.DeserializeObject<ExpandoObject>(content);
        }
    }
}
