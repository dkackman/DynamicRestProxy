using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    class RequestBuilder
    {
        private RestProxy _proxy;
        private DynamicRestClientDefaults _defaults;

        public RequestBuilder(RestProxy proxy, DynamicRestClientDefaults defaults)
        {
            Debug.Assert(proxy != null);
            _proxy = proxy;
            _defaults = defaults;
        }

        public HttpRequestMessage CreateRequest(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            var method = GetMethod(verb);

            var allNamedArgs = namedArgs.Concat(_defaults.DefaultParameters);

            var request = new HttpRequestMessage();
            request.Method = method;
            request.RequestUri = MakeUri(method, allNamedArgs);

            using (var handler = new HttpClientHandler())
            {
                request.Headers.TransferEncodingChunked = handler.SupportsTransferEncodingChunked();
            }

            var content = CreateContent(method, unnamedArgs, allNamedArgs);
            if (content != null)
            {
                request.Content = content;
            }
            
            return request;
        }

        private Uri MakeUri(HttpMethod method, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            string uri = _proxy.GetEndPointPath();

            if (method != HttpMethod.Post)
                uri += namedArgs.AsQueryString();

            return new Uri(uri, UriKind.Relative);
        }

        private static HttpContent CreateContent(HttpMethod method, IEnumerable<object> unnamedArgs, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            if (unnamedArgs.Any())
            {
                // only one object can go in the body so take the first one
                var content = new ByteArrayContent(EncodeObject(unnamedArgs.First()));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return content;
            }

            if (method == HttpMethod.Post && namedArgs.Any())
            {
                var content = new ByteArrayContent(namedArgs.AsEncodedQueryString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                return content;
            }

            return null;
        }
        private static byte[] EncodeObject(object o)
        {
            var content = JsonConvert.SerializeObject(o);
            return Encoding.UTF8.GetBytes(content);
        }

        private static HttpMethod GetMethod(string verb)
        {
            if (verb == "get")
            {
                return HttpMethod.Get;
            }
            if (verb == "post")
            {
                return HttpMethod.Post;
            }
            if (verb == "delete")
            {
                return HttpMethod.Delete;
            }
            if (verb == "put")
            {
                return HttpMethod.Put;
            }

            throw new InvalidOperationException("Unknown http verb:" + verb);
        }
    }
}
