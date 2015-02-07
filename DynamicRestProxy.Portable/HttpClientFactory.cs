using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DynamicRestProxy.PortableHttpClient
{
    static class HttpClientFactory
    {
        public static HttpClient CreateClient(Uri baseUri, DynamicRestClientDefaults defaults)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var client = new HttpClient(handler, true);

            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/x-json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));

            if (handler.SupportsTransferEncodingChunked())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = true;
            }

            if (defaults != null)
            {
                ProductInfoHeaderValue productHeader = null;
                if (!string.IsNullOrEmpty(defaults.UserAgent) && ProductInfoHeaderValue.TryParse(defaults.UserAgent, out productHeader))
                {
                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.Add(productHeader);
                }

                foreach (var kvp in defaults.DefaultHeaders)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                if (!string.IsNullOrEmpty(defaults.AuthToken) && !string.IsNullOrEmpty(defaults.AuthScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(defaults.AuthScheme, defaults.AuthToken);
                }
            }

            return client;
        }
    }
}
