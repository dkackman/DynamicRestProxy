using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

using RestSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DynamicRestProxy
{
    static class RestClientExtensions
    {
        public static async Task<T> ExecuteDynamicTaskAsync<T>(this IRestClient client, IRestRequest request, Method method, CancellationToken cancelToken, JsonSerializerSettings settings)
        {
            request.Method = method;

            if (typeof(T) != typeof(object))
            {
                var typedResponse = await client.ExecuteTaskAsync<T>(request, cancelToken);
                return typedResponse.EnsureSuccess<T>(() => typedResponse.Data);
            }

            var response = await client.ExecuteTaskAsync(request, cancelToken);
            return response.EnsureSuccess<dynamic>(() => DeserializeToDynamic(response.Content.Trim(), settings));
        }

        private static T EnsureSuccess<T>(this IRestResponse response, Func<T> successFunc)
        {
            if (response == null)
            {
                throw new TimeoutException("The server returned no response or the request timed out");
            }

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            if (string.IsNullOrEmpty(response.Content))
            {
                return default(T);
            }

            return successFunc();
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

        public static void AddDictionary(this IRestRequest request, IDictionary<string, object> args)
        {
            foreach (var kvp in args.Where(kvp => kvp.Value != null))
            {
                request.AddParameter(kvp.Key, kvp.Value.ToString());
            }
        }
    }
}

