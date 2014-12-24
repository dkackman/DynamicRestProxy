using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace DynamicRestProxy
{
    static class Extensions
    {
        public static bool IsOfType(this object o, IEnumerable<TypeInfo> types)
        {
            Debug.Assert(o != null);

            var typeInfo = o.GetType().GetTypeInfo();
            foreach (var t in types)
            {
                if (t.IsAssignableFrom(typeInfo))
                {
                    return true;
                }
            }

            return false;
        }

        public static T FirstOrNewInstance<T>(this IEnumerable<T> source) where T : new()
        {
            return source.Any() ? source.First() : new T();
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, T def)
        {
            return source.Any() ? source.First() : def;
        }

        public static string AsQueryString(this IEnumerable<KeyValuePair<string, object>> parameters, string prepend = "?")
        {
            if (!parameters.Any())
                return "";

            var builder = new StringBuilder(prepend);

            var separator = "";
            foreach (var kvp in parameters.Where(kvp => kvp.Value != null))
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
    }
}
