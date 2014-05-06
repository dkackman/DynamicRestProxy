using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    public class HttpClientProxy : RestProxy
    {
        private HttpClient _client;

        public HttpClientProxy(string baseUrl)
            : this(new HttpClient())
        {
            _client.BaseAddress = new Uri(baseUrl);
        }

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
            var request = new HttpRequestMessage(HttpMethod.Get, GetEndPointPath() + namedArgs.AsQueryString());
            request.Headers.TransferEncodingChunked = true;

            HttpResponseMessage response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(content));
            }

            return null;
        }
    }
}
