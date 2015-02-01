using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Http;

namespace DynamicRestProxy.PortableHttpClient
{
    class RequestBuilder
    {
        private readonly RestProxy _proxy;

        private static readonly IDictionary<string, HttpMethod> _methods = BinderExtensions._verbs.ToDictionary(verb => verb, verb => new HttpMethod(verb.ToUpperInvariant()));

        public RequestBuilder(RestProxy proxy)
        {
            Debug.Assert(proxy != null);
            _proxy = proxy;
        }

        public HttpRequestMessage CreateRequest(string verb, IEnumerable<object> unnamedArgs, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            // the way the base class and this class's static contructor use BinderExtensions._verbs should prevent an unkown verb from reaching here
            Debug.Assert(_methods.ContainsKey(verb), "unrecognized verb. check the BinderExtensions _verbs array");

            var method = _methods[verb];
            return new HttpRequestMessage()
            {
                Method = method,
                RequestUri = CreateUri(method, namedArgs),
                Content = CreateContent(method, unnamedArgs, namedArgs)
            };
        }

        private Uri CreateUri(HttpMethod method, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            var builder = new StringBuilder(_proxy.GetEndPointPath());

            // all methods but post put params on the url
            if (method != HttpMethod.Post)
            {
                builder.Append(namedArgs.AsQueryString());
            }
            else
            {
                // by default post uses form encoded paramters but it is allowable to have params on the url
                // see google storage api for example https://developers.google.com/storage/docs/json_api/v1/objects/insert
                // the PostUrlParam will wrap the param value and is a signal to force it onto the url and not form encode it
                builder.Append(namedArgs.Where(kvp => kvp.Value is PostUrlParam).AsQueryString());
            }

            return new Uri(builder.ToString(), UriKind.Relative);
        }

        private static HttpContent CreateContent(HttpMethod method, IEnumerable<object> unnamedArgs, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            // if there are unnamed args they represent the request body
            if (unnamedArgs.Any())
            {
                return ContentFactory.Create(unnamedArgs);
            }

            // otherwise we assume that the named args that don't go on the url represent form encoded request body
            // for post requests pass any params as form-encoded - unless forced on the query string
            var localNamedArgs = namedArgs.Where(kvp => !(kvp.Value is PostUrlParam));
            if (method == HttpMethod.Post && localNamedArgs.Any())
            {
                return ContentFactory.Create(localNamedArgs);
            }

            return null;
        }
    }
}
