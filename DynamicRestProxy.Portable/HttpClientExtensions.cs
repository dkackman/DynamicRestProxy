using System.Net.Http;
using System.Dynamic;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DynamicRestProxy.PortableHttpClient
{
    static class HttpClientExtensions
    {
        public async static Task<T> Deserialize<T>(this HttpResponseMessage response, JsonSerializerSettings settings)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
            {
                // if the return type is object return a dynamic object
                if (typeof(T) == typeof(object))
                {
                    return DeserializeToDynamic(content);
                }

                // otherwise deserialize to the return type
                return JsonConvert.DeserializeObject<T>(content, settings);
            }

            // no content - return default
            return default(T);
        }

        static dynamic DeserializeToDynamic(string content)
        {
            Debug.Assert(!string.IsNullOrEmpty(content));

            var converter = new ExpandoObjectConverter();
            if (content.StartsWith("[")) // when the result is a list we need to tell JSonConvert
            {
                return JsonConvert.DeserializeObject<List<dynamic>>(content, converter);
            }

            return JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
        }
    }
}
