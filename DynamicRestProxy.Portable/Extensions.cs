using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DynamicRestProxy.PortableHttpClient
{
    static class Extensions
    {
        public static string AsQueryString(this IEnumerable<KeyValuePair<string, object>> parameters, string prepend = "?")
        {
            if (!parameters.Any())
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

        public static byte[] AsEncodedQueryString(this IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            return namedArgs.AsEncodedQueryString(Encoding.UTF8);
        }

        public static byte[] AsEncodedQueryString(this IEnumerable<KeyValuePair<string, object>> namedArgs, Encoding encoding)
        {
            var content = namedArgs.AsQueryString("");
            return encoding.GetBytes(content);
        }
    }
}
