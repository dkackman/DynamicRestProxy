using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy.RestSharp
{
    public class RestSharpProxy : RestProxy
    {
        private IRestClient _client;

        public RestSharpProxy(string baseUri)
            : this(new RestClient(baseUri))
        {
        }

        public RestSharpProxy(IRestClient client)
            : this(client, null, "")
        {
        }

        internal RestSharpProxy(IRestClient client, RestProxy parent, string name)
            : base(parent, name)
        {
            Debug.Assert(client != null);

            _client = client;
        }

        protected override Uri BaseUri
        {
            get
            {
                return _client != null ? _client.BaseUrl : null;
            }
        }

        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new RestSharpProxy(_client, parent, name);
        }

        protected async override Task<T> CreateVerbAsyncTask<T>(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs, CancellationToken cancelToken, JsonSerializerSettings settings)
        {
            var builder = new RequestBuilder(this);
            var request = builder.BuildRequest(verb, unnamedArgs, namedArgs);

            // the binder name (i.e. the dynamic method name) is the verb
            // example: proxy.locations.get() binder.Name == "get"
            // set the result to the async task that will execute the request and create the dynamic object
            // based on the supplied verb
            return await _client.ExecuteDynamicTaskAsync<T>(request, cancelToken, settings);
        }

        internal void AddSegment(IRestRequest request)
        {
            if (Parent != null && Parent.Index != -1) // don't add a segemnt for the root element
            {
                ((RestSharpProxy)Parent).AddSegment(request);
            }

            request.AddUrlSegment(Index.ToString(), Name);
        }
    }
}
