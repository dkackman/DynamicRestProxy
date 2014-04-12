using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    interface IDynanmicEndPoint
    {
        RestRequest CreateChildRequest(int index);
    }

    class DynamicUriPart : DynamicObject, IDynanmicEndPoint
    {
        private RestClient _client;
        private IDynanmicEndPoint _parent;
        private string _name;
        private char _keywordEscapeCharacter;

        internal DynamicUriPart(RestClient client, IDynanmicEndPoint parent, string name, char keywordEscapeCharacter = '_')
        {
            _client = client;
            _parent = parent;
            _name = name;
            _keywordEscapeCharacter = keywordEscapeCharacter;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var request = CreateChildRequest(0);
            request.AddUrlSegment("0", binder.Name);
            request.AddParameters(0, binder.CallInfo.ArgumentNames, args, _keywordEscapeCharacter);
         
            // and set the result to the async task that will execute the request and create the dynamic object
            result = _client.ExecuteDynamicGetTaskAsync(request);
            return true;
        }

        public RestRequest CreateChildRequest(int index)
        {
            var request = _parent.CreateChildRequest(++index);
            request.AddUrlSegment(index.ToString(), _name);
            return request;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicUriPart(_client, this, binder.Name);

            return true;
        }
    }
}
