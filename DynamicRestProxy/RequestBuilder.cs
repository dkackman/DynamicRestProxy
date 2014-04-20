using System.Text;
using System.Diagnostics;
using System.Dynamic;

using RestSharp;

using Newtonsoft.Json;

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

            // total number of segments are all of the parent segments plus the number of unnamed arguments
            int segmentCount = _proxy.Index + 1;
            string template = CreateUrlSegmentTemplate(segmentCount);

            var request = new RestRequest(template);
            request.RequestFormat = DataFormat.Json;

            // if the binder endpoint isn't a verb (post, get etc) it represents a segment of the url - add it
            request.AddUrlSegment(segmentCount.ToString(), binder.Name.TrimStart(_proxy.KeywordEscapeCharacter));

            // fill in the url segments 
            _proxy.AddSegment(request);

            // all named arguments are added as parameters
            int unnamedArgCount = binder.UnnamedArgCount();
            SetParameters(request, binder.CallInfo, args, unnamedArgCount);

            // All unnamed args get added to the request body
            for (int i = 0; i < unnamedArgCount; i++)
            {
                request.AddBody(args[i]);
            }

            return request;
        }

        private void SetParameters(RestRequest request, CallInfo call, object[] args, int offset)
        {
            for (int i = 0; i < call.ArgumentNames.Count; i++)
            {
                request.AddParameter(call.ArgumentNames[i].TrimStart(_proxy.KeywordEscapeCharacter), args[i + offset]);
            }
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
