using System.IO;
using System.Net.Http;
using System.Dynamic;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Extension methods to aid deserialization
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Helper method to deserialize content in a number of diferent ways
        /// </summary>
        /// <typeparam name="T"> The type to deserialize to
        /// <see cref="Stream"/>
        /// <see cref="string"/>
        /// <see cref="T:System.Byte[]"/> array
        /// <see cref="ExpandoObject"/> when T is dynamic
        /// or other POCO types
        /// </typeparam>
        /// <param name="response">An <see cref="HttpResponseMessage"/> to deserialize</param>
        /// <param name="settings">Json settings to control deserialization</param>
        /// <returns>content deserialized to type T</returns>
        public async static Task<T> Deserialize<T>(this HttpResponseMessage response, JsonSerializerSettings settings)
        {
            // if the client asked for a stream or byte array, return without serializing to a different type
            if (typeof(T) == typeof(Stream))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                
                return (T)(object)stream;
            }

            if (typeof(T) == typeof(byte[]))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return (T)(object)bytes;
            }

            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
            {
                // return type is string, just return the content
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)content;
                }

                // if the return type is object return a dynamic object
                if (typeof(T) == typeof(object))
                {
                    return DeserializeToDynamic(content.Trim(), settings);
                }

                // otherwise deserialize to the return type
                return JsonConvert.DeserializeObject<T>(content, settings);
            }

            // no content - return default
            return default(T);
        }

        static dynamic DeserializeToDynamic(string content, JsonSerializerSettings settings)
        {
            Debug.Assert(!string.IsNullOrEmpty(content));

            settings.Converters.Add(new ExpandoObjectConverter());
            if (content.StartsWith("[")) // when the result is a list we need to tell JSonConvert
            {
                return JsonConvert.DeserializeObject<List<dynamic>>(content, settings);
            }

            return JsonConvert.DeserializeObject<ExpandoObject>(content, settings);
        }
    }
}
