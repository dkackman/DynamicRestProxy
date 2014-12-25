using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Dynamic;
using System.Collections.Generic;

using DynamicRestProxy.RestSharp;

using RestSharp;

namespace DynamicRestProxy
{
    class RequestBuilder
    {
        RestSharpProxy _proxy;
        public RequestBuilder(RestSharpProxy proxy)
        {
            Debug.Assert(proxy != null);
            _proxy = proxy;
        }

        public IRestRequest BuildRequest(IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            // total number of segments is the number or parts of the call chain not including the root
            // example: proxy.location.geo.get() has two url segments - the verb doesn't count
            // Index is zero based so add one
            var request = new RestRequest(CreateUrlSegmentTemplate(_proxy.Index + 1));
            request.RequestFormat = DataFormat.Json; // we only talk json            
            request.AddHeader("Accept", "application/json, text/json, text/x-json, text/javascript");
            
            // fill in the url segments with the names of each call chain member
            _proxy.AddSegment(request); // this recurses up the instance chain

            // all unnamed args get added to the request body
            foreach (var arg in unnamedArgs)
            {
                request.AddBody(arg);
            }

            foreach(var kvp in namedArgs)
            {
                if (kvp.Value is IDictionary<string, object>) // if the arg is a dictionary, add each item as a parameter key value pair
                {
                    // the name of a dictionary does not matter since we are dereferencing everything 
                    request.AddDictionary((IDictionary<string, object>)kvp.Value);
                }
                else if (kvp.Value is FileInfo) // if the arg is a file, add it as such along with the the arg name
                {
                    var file = (FileInfo)kvp.Value;
                    request.AddFile(kvp.Key, file.FullName);
                }
                else
                {
                    // everything else is key value pair as string
                    request.AddParameter(kvp.Key, kvp.Value.ToString());
                }
            }

            return request;
        }

        private static string CreateUrlSegmentTemplate(int count)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                builder.Append("/{").Append(i).Append("}");
            }
            return builder.ToString();
        }
    }
}
