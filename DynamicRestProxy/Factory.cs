using System.Text;
using System.Diagnostics;
using System.Dynamic;
using System.Collections.ObjectModel;

using RestSharp;

namespace DynamicRestProxy
{
    static class RequestFactory
    {
        public static void AddParameters(this RestRequest request, int start, ReadOnlyCollection<string> argumentNames, object[] args, char keywordEscapeCharacter = '_')
        {
            for (int i = start; i < argumentNames.Count; i++)
            {
                request.AddParameter(argumentNames[i].TrimStart(keywordEscapeCharacter), args[i]);
            }
        }

        public static RestRequest CreateReverseRequestTemplate(string name, int segmentCount)
        {
            StringBuilder builder = new StringBuilder(name);
            for (int i = segmentCount - 1; i >= 0; i--)
            {
                builder.Append("/{").Append(i).Append("}");
            }
            Debug.WriteLine(builder.ToString());
            return new RestRequest(builder.ToString());
        }

        public static RestRequest CreateRequestTemplate(string name, int segmentCount)
        {
            StringBuilder builder = new StringBuilder(name);
            for (int i = 0; i < segmentCount; i++)
            {
                builder.Append("/{").Append(i).Append("}");
            }
            Debug.WriteLine(builder.ToString());
            return new RestRequest(builder.ToString());
        }

        public static RestRequest CreateRequest(InvokeMemberBinder binder, object[] args, char keywordEscapeCharacter = '_')
        {
            int unnamedArgCount = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;

            // build up the segment template - unnamed arguments (the first in the array) are treaed as segments
            var request = CreateRequestTemplate(binder.Name, unnamedArgCount);

            // replace each index with the appropriate arg to build up the endpoint path
            for (int i = 0; i < unnamedArgCount; i++)
            {
                request.AddUrlSegment(i.ToString(), args[i].ToString());
            }

            request.AddParameters(unnamedArgCount, binder.CallInfo.ArgumentNames, args, keywordEscapeCharacter);

            return request;
        }
    }
}
