﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DynamicRestProxy.PortableHttpClient
{
    static class HttpClientFactory
    {
        public static HttpClient CreateClient(Uri baseAddress, HttpMessageHandler handler, bool disposeHandler, DynamicRestClientDefaults defaults)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            var client = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = baseAddress ?? throw new ArgumentNullException("baseAddress")
            };
            client.DefaultRequestHeaders.Accept.Clear();

            if (defaults == null || !defaults.DefaultHeaders.ContainsKey("Accept"))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/x-json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
            }

            if (defaults != null)
            {
                if (!string.IsNullOrEmpty(defaults.UserAgent) && ProductInfoHeaderValue.TryParse(defaults.UserAgent, out ProductInfoHeaderValue productHeader))
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

        public static HttpClient CreateClient(Uri baseUri, DynamicRestClientDefaults defaults)
        {
            return CreateClient(baseUri, new HttpClientHandler(), true, defaults); ;
        }
    }
}
