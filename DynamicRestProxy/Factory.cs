using System.Text;
using System.Diagnostics;
using System.Dynamic;
using System.Collections.ObjectModel;

using RestSharp;

namespace DynamicRestProxy
{
    static class ProxyExtensions
    {
        static void AddParameters(this RestRequest request, int start, ReadOnlyCollection<string> argumentNames, object[] args, char keywordEscapeCharacter = '_')
        {
            for (int i = start; i < argumentNames.Count; i++)
            {
                request.AddParameter(argumentNames[i].TrimStart(keywordEscapeCharacter), args[i]);
            }
        }

        static string CreateUrlSegmentTemplate(int count)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                builder.Append("/{").Append(i).Append("}");
            }
            Debug.WriteLine(builder.ToString());
            return builder.ToString();
        }

        public static RestRequest CreateRequest(this RestProxy segment, InvokeMemberBinder binder, object[] args, char keywordEscapeCharacter = '_')
        {
            int unnamedArgCount = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
            // total number of segments are all of the parent segments plus the number of unnamed arguments
            int segments = segment.SegmentCount;
            int count = segments + unnamedArgCount;
            string template = CreateUrlSegmentTemplate(count + 1); // plus one is for the binder endpoint;

            var request = new RestRequest(template);

            // the binder endpoint isn't a dynamicurlsegment so is at index + 1 of this segment
            request.AddUrlSegment(segments.ToString(), binder.Name.TrimStart(keywordEscapeCharacter));

            // fill in the url segments for this object and its parents
            segment.AddSegment(request);

            // fill in the url segments passed as unnamed arguments to the dynamic invocation
            for (int i = segments; i < count; i++)
            {
                request.AddUrlSegment((i + 1).ToString(), args[i - segments].ToString());
            }

            // now add all named arguments as parameters
            for (int i = 0; i < binder.CallInfo.ArgumentNames.Count; i++)
            {
                request.AddParameter(binder.CallInfo.ArgumentNames[i].TrimStart(keywordEscapeCharacter), args[i + unnamedArgCount]);
            }

            return request;
        }
    }
}
