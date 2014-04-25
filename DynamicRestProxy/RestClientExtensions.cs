using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    static class RestClientExtensions
    {
        public static async Task<dynamic> ExecuteDynamicGetTaskAsync(this IRestClient client, IRestRequest request)
        {
            var response = await client.ExecuteGetTaskAsync(request);
            if (response == null)
                return null;

            return await response.Deserialize();
        }

        public static async Task<dynamic> ExecuteDynamicPostTaskAsync(this IRestClient client, IRestRequest request)
        {
            var response = await client.ExecutePostTaskAsync(request);
            if (response == null)
                return null;

            return await response.Deserialize();
        }

        public static async Task<dynamic> DynamicDeleteTaskAsync(this IRestClient client, IRestRequest request)
        {

            request.Method = Method.DELETE;
            var response = await client.ExecuteTaskAsync<dynamic>(request);
            if (response == null)
                return null;

            return await response.Deserialize();
        }

        public static async Task<dynamic> DynamicPutTaskAsync(this IRestClient client, IRestRequest request)
        {
            request.Method = Method.PUT;
            var response = await client.ExecuteTaskAsync<dynamic>(request);
            if (response == null)
                return null;
            
            return await response.Deserialize();
        }

        public static async Task<dynamic> Deserialize(this IRestResponse response)
        {
            if (!string.IsNullOrEmpty(response.Content))
            {
                return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(response.Content));
            }
            return null;
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
