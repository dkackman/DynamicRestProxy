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

        public RestSharpProxy(string baseUrl)
            : this(new RestClient(baseUrl))
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

        protected override Uri BaseUrl
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

        protected async override Task<T> CreateVerbAsyncTask<T>(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs, CancellationToken cancelToken, JsonSerializerSettings serializationSettings)
        {
            var builder = new RequestBuilder(this);
            var request = builder.BuildRequest(unnamedArgs, namedArgs);

            // the binder name (i.e. the dynamic method name) is the verb
            // example: proxy.locations.get() binder.Name == "get"
            var invocation = new RestInvocation(_client, verb);
            return await invocation.InvokeAsync<T>(request, cancelToken, serializationSettings); // this will return a Task<T> with the rest async call
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
