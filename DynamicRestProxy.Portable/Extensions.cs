using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace DynamicRestProxy.PortableHttpClient
{
    static class Extensions
    {
        public static string AsQueryString(this IDictionary<string, object> parameters, string prepend = "?")
        {
            if (parameters.Count == 0)
                return "";

            var builder = new StringBuilder(prepend);

            var separator = "";
            foreach(var kvp in parameters.Where(kvp => kvp.Value != null))
            {
                if (kvp.Value is IDictionary<string, object>)
                {
                    // since we can pass escaped parameters in a dictionary recurse if the value is a dictionary
                    builder.Append(((IDictionary<string, object>)kvp.Value).AsQueryString(""));
                }
                else
                {
                    builder.AppendFormat("{0}{1}={2}", separator, WebUtility.UrlEncode(kvp.Key), WebUtility.UrlEncode(kvp.Value.ToString()));
                }
                separator = "&";
            }
            return builder.ToString();
        }

        public static byte[] AsEncodedQueryString(this IDictionary<string, object> namedArgs)
        {
            return namedArgs.AsEncodedQueryString(Encoding.UTF8);
        }

        public static byte[] AsEncodedQueryString(this IDictionary<string, object> namedArgs, Encoding encoding)
        {
            var content = namedArgs.AsQueryString("");
            return encoding.GetBytes(content);
        }
    }
}
