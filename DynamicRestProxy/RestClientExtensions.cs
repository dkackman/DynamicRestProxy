using System.Threading.Tasks;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    static class RestClientExtensions
    {
        public static async Task<dynamic> ExecuteDynamicGetTaskAsync(this RestClient client, RestRequest request)
        {
            var response = await client.ExecuteGetTaskAsync(request);
            if (response == null)
                return null;

            return await response.Deserialize();
        }

        public static async Task<dynamic> ExecuteDynamicPostTaskAsync(this RestClient client, RestRequest request)
        {
            var response = await client.ExecutePostTaskAsync(request);
            if (response == null)
                return null;

            return await response.Deserialize();
        }

        public static async Task<dynamic> DynamicDeleteTaskAsync(this RestClient client, RestRequest request)
        {
            request.Method = Method.DELETE;
            var response = await client.ExecuteTaskAsync<dynamic>(request);
            if (response == null)
                return null;

            return await response.Deserialize();
        }

        public static async Task<dynamic> DynamicPutTaskAsync(this RestClient client, RestRequest request)
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
                return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(response.Content));

            return null;
        }
    }
}
