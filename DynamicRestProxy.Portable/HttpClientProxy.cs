using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace DynamicRestProxy.PortableHttpClient
{
    public class HttpClientProxy : RestProxy
    {
        private HttpClient _client;

        public HttpClientProxy(HttpClient client)
            : this(client, null, "")
        {
        }

        internal HttpClientProxy(HttpClient client, RestProxy parent, string name)
            : base(parent, name)
        {
            Debug.Assert(client != null);

            _client = client;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/x-json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
        }

        protected override string BaseUrl
        {
            get { return _client.BaseAddress.ToString(); }
        }

        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new HttpClientProxy(_client, parent, name);
        }

        protected async override Task<dynamic> CreateVerbAsyncTask(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            var builder = new RequestBuilder(this);
            using (var request = builder.CreateRequest(verb, unnamedArgs, namedArgs))
            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                return await response.Deserialize();
            }
        }
    }
}
