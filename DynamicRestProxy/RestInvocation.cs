using System;
using System.Threading.Tasks;
using System.Diagnostics;

using RestSharp;

namespace DynamicRestProxy
{
    class RestInvocation
    {
        public RestInvocation(string verb = "get")
        {
            Verb = verb;
        }

        public string Verb { get; set; }

        public async Task<dynamic> InvokeAsync(RestClient client, RestRequest request)
        {
            Debug.Assert(client != null);

            // set the result to the async task that will execute the request and create the dynamic object
            // based on the supplied verb
            if (Verb == "post")
            {
                return await client.ExecuteDynamicPostTaskAsync(request);
            }
            else if (Verb == "get")
            {
                return await client.ExecuteDynamicGetTaskAsync(request);
            }
            else if (Verb == "delete")
            {
                return await client.DynamicDeleteTaskAsync(request);
            }

            Debug.Assert(false, "unsupported verb");
            throw new InvalidOperationException("unsupported verb: " + Verb);
        }
    }
}