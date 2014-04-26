using System;
using System.Text;
using System.Dynamic;
using System.Diagnostics;

using RestSharp;

namespace DynamicRestProxy
{
    public class RestProxy : DynamicObject
    {
        private IRestClient _client;
        private RestProxy _parent;
        private string _name;

        internal char KeywordEscapeCharacter { get; private set; }

        public RestProxy(IRestClient client, char keywordEscapeCharacter = '_')
            : this(client, null, "", keywordEscapeCharacter)
        {
        }

        internal RestProxy(IRestClient client, RestProxy parent, string name, char keywordEscapeCharacter)
        {
            Debug.Assert(client != null);

            _client = client;
            _parent = parent;
            _name = name;
            KeywordEscapeCharacter = keywordEscapeCharacter;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            Debug.Assert(binder != null);

            if (args.Length != 1)
                throw new InvalidOperationException("The segment escape sequence must have exactly 1 unnamed parameter");

            // this is called when the dynamic object is invoked like a delagate
            // dynamic segment1 = proxy.segment1;
            // dynamic chain = segment1("escaped"); <- this calls TryInvoke
            result = new RestProxy(_client, this, args[0].ToString(), KeywordEscapeCharacter);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Debug.Assert(binder != null);
            Debug.Assert(args != null);

            if (binder.IsVerb())
            {
                // build a rest request based on this instance, parent instances and invocation arguments
                var builder = new RequestBuilder(this);
                var request = builder.BuildRequest(binder, args);

                // the binder name (i.e. the dynamic method name) is the verb
                // example: proxy.locations.get() binder.Name == "get"
                var invocation = new RestInvocation(_client, binder.Name);
                result = invocation.InvokeAsync(request); // this will return a Task<dynamic> with the rest async call
            }
            else
            {
                if (args.Length != 1)
                    throw new InvalidOperationException("The segment escape sequence must have exactly 1 unnamed parameter");

                // this is for when we escape a url segment by passing it as an argument to a method invocation
                // example: proxy.segment1("escaped")
                // here we create two new dynamic objects, 1 for "segment1" which is the method name
                // and then we create one for the escaped segment passed as an argument - "escaped" in the example
                var tmp = new RestProxy(_client, this, binder.Name, KeywordEscapeCharacter);
                result = new RestProxy(_client, tmp, args[0].ToString(), KeywordEscapeCharacter);
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(binder != null);

            // this gets invoked when a dynamic property is accessed
            // example: proxy.locations will invoke here with a binder named locations
            // each dynamic property is treated as a url segment
            result = new RestProxy(_client, this, binder.Name, KeywordEscapeCharacter);

            return true;
        }

        internal int Index
        {
            get
            {
                return _parent != null ? _parent.Index + 1 : -1; // the root is the main url - does not represent a url segment
            }
        }

        internal void AddSegment(IRestRequest request)
        {
            if (_parent != null && _parent.Index != -1) // don't add a segemnt for the root element
            {
                _parent.AddSegment(request);
            }

            request.AddUrlSegment(Index.ToString(), _name);
        }

        private void ToString(StringBuilder builder)
        {
            if (_parent != null)
                _parent.ToString(builder);

            if (string.IsNullOrEmpty(_name)) // if _name is null we are the root
            {
                builder.Append(_client.BaseUrl);
            }
            else
            {
                builder.Append("/").Append(_name);
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            ToString(builder);
            return builder.ToString();
        }
    }
}
