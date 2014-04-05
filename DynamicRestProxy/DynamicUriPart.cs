using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    class DynamicUriPart : DynamicObject
    {
        private string _name;
        private RestClient _client;
        private char _keywordEscapeCharacter;

        internal DynamicUriPart(RestClient client, string name, char keywordEscapeCharacter = '_')
        {
            _client = client;
            _name = name;
            _keywordEscapeCharacter = keywordEscapeCharacter;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var request = Factory.CreateRequest(binder, args, _keywordEscapeCharacter);
            result = null;
            // and set the result to the async task that will execute the request and create the dynamic object
           // result = _client.ExecuteDynamicGetTaskAsync(request);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicUriPart(_client, binder.Name);

            return true;
        }
    }
}
