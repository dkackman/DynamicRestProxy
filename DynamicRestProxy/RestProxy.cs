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
            var request = this.CreateRequest(binder, args, _keywordEscapeCharacter);

            // set the result to the async task that will execute the request and create the dynamic object
            result = _client.ExecuteDynamicGetTaskAsync(request);

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
                return _parent != null ? _parent.Index + 1 : 0;
            }
        }

        internal int SegmentCount
        {
            get
            {
                return Index + 1;
            }
        }

        internal void AddSegment(RestRequest request)
        {
            if (_parent != null)
            {
                _parent.AddSegment(request);
            }

            request.AddUrlSegment(Index.ToString(), _name.TrimStart(_keywordEscapeCharacter));
        }
    }
}
