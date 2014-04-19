using System.Threading.Tasks;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    static class RestClientExtensions
    {
        public static async Task<dynamic> Deserialize(this IRestResponse response)
        {
            if (!string.IsNullOrEmpty(response.Content))
                return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(response.Content));

            return null;
        }

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
            var response = await client.DeleteTaskAsync<dynamic>(request);
            if (response == null)
                return null;

            return await response.Deserialize();
        }
    }
}
