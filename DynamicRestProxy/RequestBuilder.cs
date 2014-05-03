using System.IO;
using System.Text;
using System.Diagnostics;
using System.Dynamic;
using System.Collections.Generic;

using RestSharp;

namespace DynamicRestProxy
{
    class RequestBuilder
    {
        RestProxy _proxy;
        public RequestBuilder(RestProxy proxy)
        {
            Debug.Assert(proxy != null);
            _proxy = proxy;
        }

        public RestRequest BuildRequest(InvokeMemberBinder binder, object[] args)
        {
            Debug.Assert(binder.IsVerb());

            // total number of segments is the number or parts of the call chain not including the root
            // example: proxy.location.geo.get() has two url segments - the verb doesn't count
            // Index is zero based so add one
            string template = CreateUrlSegmentTemplate(_proxy.Index + 1);

            var request = new RestRequest(template);
            request.RequestFormat = DataFormat.Json; // we only talk json            
            request.AddHeader("Accept", "application/json, text/json, text/x-json, text/javascript");
            
            // fill in the url segments with the names of each call chain member
            _proxy.AddSegment(request); // this recurses up the instance chain

            int unnamedArgCount = binder.UnnamedArgCount();

            // all named arguments are added as parameters
            for (int i = 0; i < binder.CallInfo.ArgumentNames.Count; i++)
            {
                var arg = args[i + unnamedArgCount];
                if (arg is IDictionary<string, object>) // if the arg is a dictionary, add each item as a parameter key value pair
                {
                    request.AddDictionary((IDictionary<string, object>)arg);
                }
                else if (arg is FileInfo) // if the arg is a file, add it as such along with the the arg name
                {
                    var file = (FileInfo)arg;
                    request.AddFile(binder.GetArgName(i, _proxy.KeywordEscapeCharacter), file.FullName);
                }
                else
                {
                    request.AddParameter(binder.GetArgName(i, _proxy.KeywordEscapeCharacter), arg);
                }
            }

            // all unnamed args get added to the request body
            for (int i = 0; i < unnamedArgCount; i++)
            {
                request.AddBody(args[i]);
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
