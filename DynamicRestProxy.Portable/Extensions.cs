using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace DynamicRestProxy.PortableHttpClient
{
    static class Extensions
    {
        public static string AsQueryString(this IDictionary<string, object> parameters, string prepend = "?")
        {
            var builder = new StringBuilder();

            var separator = "";
            foreach(var kvp in parameters.Where(kvp => kvp.Value != null))
            {
                if (kvp.Value is IDictionary<string, object>)
                {
                    builder.Append(((IDictionary<string, object>)kvp.Value).AsQueryString(""));
                }
                else
                {
                    builder.AppendFormat("{0}{1}={2}", separator, WebUtility.UrlEncode(kvp.Key), WebUtility.UrlEncode(kvp.Value.ToString()));
                }
                separator = "&";
            }
            return prepend + builder.ToString();
        }
    }
}
