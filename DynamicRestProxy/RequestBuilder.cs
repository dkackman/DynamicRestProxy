using System.Text;
using System.Diagnostics;
using System.Dynamic;

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
            // TODO - refactor this -- too much index management

            //unnamed arguments are treated as url segments; named arguments are parameters
            int unnamedArgCount = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
            // total number of segments are all of the parent segments plus the number of unnamed arguments
            int parentCount = _proxy.Index + 1;
            int count = parentCount + unnamedArgCount;
            string template = CreateUrlSegmentTemplate(count + binder.UrlSegmentCount());

            var request = new RestRequest(template);

            // if the binder endpoint isn't a verb (post, get etc) it represents a segment of the url - add it
            if (!binder.IsVerb())
            {
                request.AddUrlSegment(parentCount.ToString(), binder.Name.TrimStart(_proxy.KeywordEscapeCharacter));
            }

            // fill in the url segments 
            SetSegments(request, parentCount - binder.UrlSegmentOffset(), count - binder.UrlSegmentOffset(), args);

            // now add all named arguments as parameters
            SetParameters(request, binder.CallInfo, args, unnamedArgCount);

            return request;
        }

        private void SetSegments(RestRequest request, int start, int count, object[] args)
        {
            // fill in the url segments for this object and its parents
            _proxy.AddSegment(request);

            // fill in the url segments passed as unnamed arguments to the dynamic invocation
            for (int i = start; i < count; i++)
            {
                request.AddUrlSegment((i + 1).ToString(), args[i - start].ToString());
            }
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
