using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Dynamic;

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

            // request = CreateRequest(binder, args);
            if (Verb == "post")
            {
                // set the result to the async task that will execute the request and create the dynamic object
                return await client.ExecuteDynamicPostTaskAsync(request);
            }
            else if (Verb == "get")
            {
                // set the result to the async task that will execute the request and create the dynamic object
                return await client.ExecuteDynamicGetTaskAsync(request);
            }

            Debug.Assert(false, "unsupported verb");
            throw new InvalidOperationException("unsupported verb: " + Verb);
        }
    }
}