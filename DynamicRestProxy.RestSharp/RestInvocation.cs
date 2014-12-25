using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    class RestInvocation
    {
        private readonly IRestClient _client;

        public RestInvocation(IRestClient client, string verb)
        {
            Debug.Assert(client != null);
            _client = client;
            Verb = verb;
        }

        public string Verb { get; private set; }

        public async Task<T> InvokeAsync<T>(IRestRequest request, CancellationToken cancelToken, JsonSerializerSettings settings)
        {
            // set the result to the async task that will execute the request and create the dynamic object
            // based on the supplied verb
            if (Verb == "post")
            {
                return await _client.ExecuteDynamicTaskAsync<T>(request, Method.POST, cancelToken, settings);
            }
            else if (Verb == "get")
            {
                return await _client.ExecuteDynamicTaskAsync<T>(request, Method.GET, cancelToken, settings);
            }
            else if (Verb == "delete")
            {
                return await _client.ExecuteDynamicTaskAsync<T>(request, Method.DELETE, cancelToken, settings);
            }
            else if (Verb == "put")
            {
                return await _client.ExecuteDynamicTaskAsync<T>(request, Method.PUT, cancelToken, settings);
            }
            else if (Verb == "patch")
            {
                return await _client.ExecuteDynamicTaskAsync<T>(request, Method.PATCH, cancelToken, settings);
            }

            Debug.Assert(false, "unsupported verb");
            throw new InvalidOperationException("unsupported verb: " + Verb);
        }
    }
}