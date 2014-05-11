using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DynamicRestProxy.PortableHttpClient
{
    public class DynamicRestClient : RestProxy
    {
        private string _baseUrl;

        public DynamicRestClient(string baseUrl)
            : this(baseUrl, null, "")
        {
        }
        internal DynamicRestClient(string baseUrl, RestProxy parent, string name)
            : base(parent, name)
        {
            _baseUrl = baseUrl;
        }

        protected override string BaseUrl
        {
            get { return _baseUrl; }
        }

        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new DynamicRestClient(_baseUrl, parent, name);
        }

        protected async override Task<dynamic> CreateVerbAsyncTask(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            using (var client = CreateClient())
            {
                var builder = new RequestBuilder(this);
                using (var request = builder.CreateRequest(verb, unnamedArgs, namedArgs))
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    return await response.Deserialize();
                }
            }
        }

        private HttpClient CreateClient()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var client = new HttpClient(handler, true);

            client.BaseAddress = new Uri(_baseUrl, UriKind.Absolute);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/x-json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));

            return client;
        }
    }
}
