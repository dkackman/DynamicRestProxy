using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    public class Service : DynamicObject
    {
        private RestClient _client;
        private Dictionary<string, DynamicUriPart> _properties = new Dictionary<string, DynamicUriPart>();

        public Service(RestClient client)
        {
            _client = client;
        }

        private async Task<dynamic> ExecuteAsync(RestRequest request)
        {
            var response = await _client.ExecuteGetTaskAsync(request);
            return response.Content.DeserializeDynamic();
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
                request.AddParameter(binder.CallInfo.ArgumentNames[i], args[i]);
            }
            var s = request.ToString();
            result = ExecuteAsync(request);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_properties.ContainsKey(binder.Name))
                _properties.Add(binder.Name, new DynamicUriPart(binder.Name));

            result = _properties[binder.Name];
            return true;
        }
    }
}
