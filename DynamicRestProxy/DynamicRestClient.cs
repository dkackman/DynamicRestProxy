using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    public class DynamicRestClient : DynamicObject, IDynanmicEndPoint
    {
        private RestClient _client;
        private char _keywordEscapeCharacter;

        public DynamicRestClient(RestClient client, char keywordEscapeCharacter = '_')
        {
            _client = client;
            _keywordEscapeCharacter = keywordEscapeCharacter;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var request = RequestFactory.CreateRequest(binder, args, _keywordEscapeCharacter);

            // set the result to the async task that will execute the request and create the dynamic object
            result = _client.ExecuteDynamicGetTaskAsync(request);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicUriPart(_client, this, binder.Name);

            return true;
        }

        public RestRequest CreateChildRequest(int index)
        {
            return RequestFactory.CreateReverseRequestTemplate("", index + 1);
        }
    }
}
