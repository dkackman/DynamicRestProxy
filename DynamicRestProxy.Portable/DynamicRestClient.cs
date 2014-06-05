using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DynamicRestProxy.PortableHttpClient
{
    public class DynamicRestClient : RestProxy
    {
        private string _baseUrl;
        private DynamicRestClientDefaults _defaults;
        private Func<HttpRequestMessage, Task> _configureRequest;

        public DynamicRestClient(string baseUrl, DynamicRestClientDefaults defaults = null, Func<HttpRequestMessage, Task> configure = null)
            : this(baseUrl, null, "", defaults, configure)
        {
        }

        internal DynamicRestClient(string baseUrl, RestProxy parent, string name, DynamicRestClientDefaults defaults, Func<HttpRequestMessage, Task> configure)
            : base(parent, name)
        {
            _baseUrl = baseUrl;
            _defaults = defaults ?? new DynamicRestClientDefaults(); ;
            _configureRequest = configure;
        }

        protected override string BaseUrl
        {
            get { return _baseUrl; }
        }

        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new DynamicRestClient(_baseUrl, parent, name, _defaults, _configureRequest);
        }

        protected async override Task<dynamic> CreateVerbAsyncTask(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            using (var client = CreateClient())
            {
                var builder = new RequestBuilder(this, _defaults);
                using (var request = builder.CreateRequest(verb, unnamedArgs, namedArgs))
                {
                    // give the user code a chance to setup any other request details
                    // this is especially useful for setting oauth tokens when they have different lifetimes than the rest client
                    if (_configureRequest != null)
                    {
                        await _configureRequest(request);
                    }

                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();

                        return await response.Deserialize();
                    }
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

            if (_defaults != null)
            {
                foreach (var kvp in _defaults.DefaultHeaders)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                if (!string.IsNullOrEmpty(_defaults.OAuthToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _defaults.OAuthToken);
                }
            }

            return client;
        }
    }
}