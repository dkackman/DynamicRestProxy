using System;
using System.Threading.Tasks;
using System.Diagnostics;

using RestSharp;

namespace DynamicRestProxy
{
    class RestInvocation
    {
        private IRestClient _client;

        public RestInvocation(IRestClient client, string verb)
        {
            Debug.Assert(client != null);
            _client = client;
            Verb = verb;
        }

        public string Verb { get; private set; }

        public async Task<dynamic> InvokeAsync(IRestRequest request)
        {
            // set the result to the async task that will execute the request and create the dynamic object
            // based on the supplied verb
            if (Verb == "post")
            {
                return await _client.ExecuteDynamicTaskAsync(request, Method.POST);
            }
            else if (Verb == "get")
            {
                return await _client.ExecuteDynamicTaskAsync(request, Method.GET);
            }
            else if (Verb == "delete")
            {
                return await _client.ExecuteDynamicTaskAsync(request, Method.DELETE);
            }
            else if (Verb == "put")
            {
                return await _client.ExecuteDynamicTaskAsync(request, Method.PUT);
            }

            Debug.Assert(false, "unsupported verb");
            throw new InvalidOperationException("unsupported verb: " + Verb);
        }
    }
}