using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    static class ContentFactory
    {
        public static HttpContent CreateContent(HttpMethod method, IEnumerable<object> unnamedArgs, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            // if there are unnamed args they represent the request body
            if (unnamedArgs.Any())
            {
                return Create(unnamedArgs);
            }

            // otherwise we assume that the named args that don't go on the url represent form encoded request body
            // for post requests pass any params as form-encoded - unless forced on the query string
            // also filter out named args where the value is null
            var localNamedArgs = namedArgs.Where(kvp => kvp.Value != null && !(kvp.Value is PostUrlParam));
            if (method == HttpMethod.Post && localNamedArgs.Any())
            {
                return new FormUrlEncodedContent(localNamedArgs.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.ToString())));
            }

            return null;
        }

        private static HttpContent Create(IEnumerable<object> contents)
        {
            Debug.Assert(contents != null);
            Debug.Assert(contents.Any(o => o != null));

            // if only 1 object in the collection, just create as normal
            if (contents.Count() == 1)
            {
                return Create(contents.First());
            }

            // otherwise package everything as multipart content
            var content = new MultipartFormDataContent();
            foreach (var o in contents)
            {
                content.Add(Create(o));
            }

            return content;
        }

        private static HttpContent Create(object content)
        {
            Debug.Assert(content != null);

            if (content is HttpContent) // if caller went to the trouble of passing HttpContent, just use it
            {
                return (HttpContent)content;
            }

            // check for a set of sentinal types that will serialize in a specific manner
            if (content is Stream)
            {
                return new StreamContent((Stream)content);
            }

            if (content is byte[])
            {
                return new ByteArrayContent((byte[])content);
            }

            if (content is string)
            {
                return new StringContent((string)content);
            }

            if (content is ContentInfo)
            {
                return Create((ContentInfo)content);
            }

            // primitive types (int, float etc) types get serialized as a string
            if (content.GetType().GetTypeInfo().IsPrimitive)
            {
                return new StringContent(content.ToString());
            }

            // all other types get serialized as json
            var json = JsonConvert.SerializeObject(content);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static HttpContent Create(ContentInfo info)
        {
            // create content object as normal
            var content = Create(info.Content);

            // set any additional headers
            if (!string.IsNullOrEmpty(info.MimeType))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(info.MimeType);
            }

            foreach (var kvp in info.ContentHeaders)
            {
                content.Headers.Add(kvp.Key, kvp.Value);
            }

            return content;
        }
    }
}
