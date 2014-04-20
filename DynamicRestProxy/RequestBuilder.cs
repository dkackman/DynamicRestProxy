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
            Debug.Assert(binder.IsVerb());

            // total number of segments is the number or parts of the call chain not including the root
            // example: proxy.location.geo.get() has two url segments - the verb doesn't count
            // Index is zero based so add one
            string template = CreateUrlSegmentTemplate(_proxy.Index + 1);

            var request = new RestRequest(template);
            request.RequestFormat = DataFormat.Json; // we only talk json            

            // fill in the url segments with the names of each call chain member
            _proxy.AddSegment(request);

            int unnamedArgCount = binder.UnnamedArgCount();

            // all named arguments are added as parameters
            for (int i = 0; i < binder.CallInfo.ArgumentNames.Count; i++)
            {
                request.AddParameter(binder.CallInfo.ArgumentNames[i].TrimStart(_proxy.KeywordEscapeCharacter), args[i + unnamedArgCount]);
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
