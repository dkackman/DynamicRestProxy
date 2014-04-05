using System.Text;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    static class Factory
    {
        public static RestRequest CreateRequest(InvokeMemberBinder binder, object[] args, char keywordEscapeCharacter = '_')
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
                request.AddParameter(binder.CallInfo.ArgumentNames[i].TrimStart(keywordEscapeCharacter), args[i]);
            }

            return request;
        }
    }
}
