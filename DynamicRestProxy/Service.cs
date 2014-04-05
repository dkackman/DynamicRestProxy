using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    public class Service : DynamicObject
    {
        private RestClient _client;

        public Service(RestClient client)
        {
            _client = client;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            int unnamedArgCount = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
            // build up the segment template - unnamed arguments (the first in the array) are treaed as segments
            StringBuilder builder = new StringBuilder(binder.Name);
            for (int i = 0; i < unnamedArgCount; i++)
            {
                builder.Append("/{").Append(i).Append("}");
            }

            var request = new RestRequest(builder.ToString());
            
            // replace each index with the appropriate arg to build up the endpoint path
            for (int i = 0; i < unnamedArgCount; i++)
            {
                request.AddUrlSegment(i.ToString(), args[i].ToString());
            }

            // now go through the named arguments and add as url parameters
            for (int i = unnamedArgCount; i < binder.CallInfo.ArgumentNames.Count; i++)
            {
                request.AddParameter(binder.CallInfo.ArgumentNames[i].TrimStart('_'), args[i]);
            }

            // and set the result to the async task that will execute the request and create the dynamic object
            result = _client.ExecuteDynamicGetTaskAsync(request);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicUriPart(binder.Name);

            return true;
        }
    }
}
