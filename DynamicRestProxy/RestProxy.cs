using System.Text;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    public class RestProxy : DynamicObject
    {
        private RestClient _client;
        private RestProxy _parent;
        private string _name;
        private char _keywordEscapeCharacter;

        public RestProxy(RestClient client, char keywordEscapeCharacter = '_')
            : this(client, null, "", keywordEscapeCharacter)
        {
        }

        internal RestProxy(RestClient client, RestProxy parent, string name, char keywordEscapeCharacter)
        {
            _client = client;
            _parent = parent;
            _name = name;
            _keywordEscapeCharacter = keywordEscapeCharacter;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            RestRequest request = null;
            if (binder.IsVerb())
            {
                request = CreateRequest(binder, args);

                // set the result to the async task that will execute the request and create the dynamic object
                result = _client.ExecuteDynamicPostTaskAsync(request);
            }
            else
            {
                request = CreateRequest(binder, args);

                // set the result to the async task that will execute the request and create the dynamic object
                result = _client.ExecuteDynamicGetTaskAsync(request);
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new RestProxy(_client, this, binder.Name, _keywordEscapeCharacter);

            return true;
        }

        internal int Index
        {
            get
            {
                return _parent != null ? _parent.Index + 1 : -1; // the root is the main url - does not represent a url segment
            }
        }

        internal void AddSegment(RestRequest request)
        {
            if (_parent != null && _parent.Index != -1) // don't add a segemnt for the root element
            {
                _parent.AddSegment(request);
            }

            request.AddUrlSegment(Index.ToString(), _name.TrimStart(_keywordEscapeCharacter));
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

        private RestRequest CreateRequest(InvokeMemberBinder binder, object[] args)
        {
            //unnamed arguments are treated as url segments; named arguments are parameters
            int unnamedArgCount = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
            // total number of segments are all of the parent segments plus the number of unnamed arguments
            int parentCount = Index + 1;
            int count = parentCount + unnamedArgCount;
            string template = CreateUrlSegmentTemplate(count + binder.UrlSegmentOffset()); 

            var request = new RestRequest(template);

            // the binder endpoint isn't a verb (post etc) it represents a segment of the url - add it
            if (!binder.IsVerb())
                request.AddUrlSegment(parentCount.ToString(), binder.Name.TrimStart(_keywordEscapeCharacter));

            // fill in the url segments 
            SetSegments(request, parentCount, count, args);

            // now add all named arguments as parameters
            SetParameters(request, binder.CallInfo, args, unnamedArgCount);

            return request;
        }

        private void SetSegments(RestRequest request, int start, int count, object[] args)
        {
            // fill in the url segments for this object and its parents
            AddSegment(request);

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
                request.AddParameter(call.ArgumentNames[i].TrimStart(_keywordEscapeCharacter), args[i + offset]);
            }
        }

    }
}
