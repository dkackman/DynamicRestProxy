using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    static class ContentFactory
    {
        public static HttpContent Create(IEnumerable<object> contents)
        {
            Debug.Assert(contents != null);
            Debug.Assert(contents.Any());

            if (contents.Count() == 1)
            {
                return Create(contents.First());
            }

            var content = new MultipartFormDataContent();
            foreach (var o in contents.Where(o => o != null))
            {
                content.Add(Create(o));
            }
            return content;
        }

        public static HttpContent Create(object content)
        {
            Debug.Assert(content != null);

            if (content is Stream)
            {
                return CreateFromStream((Stream)content);
            }

            if (content is StreamInfo)
            {
                var info = (StreamInfo)content;
                return CreateFromStream(info.Stream, info.MimeType);
            }

            return CreateFromObject(content);
        }

        private static HttpContent CreateFromStream(Stream content, string mimeType = "application/octet-stream")
        {
            Debug.Assert(content != null);
            var streamContent = new StreamContent(content, (int)content.Length);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            return streamContent;
        }

        private static HttpContent CreateFromObject(object content)
        {
            Debug.Assert(content != null);
            var json = JsonConvert.SerializeObject(content);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
