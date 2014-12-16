using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace DynamicRestProxy.PortableHttpClient
{
    class RequestBuilder
    {
        private readonly RestProxy _proxy;
        private readonly DynamicRestClientDefaults _defaults;

        private static readonly IDictionary<string, HttpMethod> _methods = new Dictionary<string, HttpMethod>();

        static RequestBuilder()
        {
            foreach(var verb in BinderExtensions._verbs)
            {
                _methods.Add(verb, new HttpMethod(verb.ToUpperInvariant()));
            }
        }

        public RequestBuilder(RestProxy proxy, DynamicRestClientDefaults defaults)
        {
            Debug.Assert(proxy != null);
            _proxy = proxy;
            _defaults = defaults;
        }

        public HttpRequestMessage CreateRequest(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            HttpMethod method = null;
            if (!_methods.TryGetValue(verb, out method))
            {
                throw new InvalidOperationException("Unrecognized verb: " + verb);
            }

            var allNamedArgs = namedArgs.Concat(_defaults.DefaultParameters);

            var request = new HttpRequestMessage();
            request.Method = method;
            request.RequestUri = MakeUri(method, allNamedArgs);

            // filter out a cancellationtoken if passed
            var content = CreateContent(method, unnamedArgs.Where(arg => !(arg is CancellationToken)), allNamedArgs);
            if (content != null)
            {
                request.Content = content;
            }

            return request;
        }

        private Uri MakeUri(HttpMethod method, IEnumerable<KeyValuePair<string, object>> namedArgs)
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
