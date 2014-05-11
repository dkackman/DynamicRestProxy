using System.Threading.Tasks;
using System.Net.Http;

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
