using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    static class ContentFactory
    {
        public static HttpContent Create(object content)
        {
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
            var streamContent = new StreamContent(content, (int)content.Length);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            return streamContent;
        }

        private static HttpContent CreateFromObject(object content)
        {
            var bytes = GetJsonBytes(content);
            var httpContent = new ByteArrayContent(bytes);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpContent.Headers.ContentLength = bytes.Length;
            
            return httpContent;
        }

        private static byte[] GetJsonBytes(object o)
        {
            var content = JsonConvert.SerializeObject(o);
            return Encoding.UTF8.GetBytes(content);
        }
    }
}
