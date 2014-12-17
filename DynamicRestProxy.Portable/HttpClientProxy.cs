using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Proxy wrapper around an HttpClient instance
    /// </summary>
    public class HttpClientProxy : RestProxy
    {
        private HttpClient _client;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="client">The HttpClient to wrap</param>
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

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.BaseUrl"/>
        /// </summary>
        protected override string BaseUrl
        {
            get { return _client.BaseAddress.ToString(); }
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.CreateProxyNode(RestProxy, string)"/>
        /// </summary>
        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new HttpClientProxy(_client, parent, name);
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.CreateVerbAsyncTask(string, IEnumerable{object}, IDictionary{string, object})"/>
        /// </summary>
        protected async override Task<dynamic> CreateVerbAsyncTask(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            var builder = new RequestBuilder(this, new DynamicRestClientDefaults());
            using (var request = builder.CreateRequest(verb, unnamedArgs, namedArgs))
            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                return await response.Deserialize();
            }
        }
    }
}
