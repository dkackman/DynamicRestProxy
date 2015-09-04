using System;
using System.Net.Http;

namespace DynamicRestProxy.PortableHttpClient
{
    public class DynamicRestClientResponseException : Exception
    {
        public HttpResponseMessage Response { get; private set; }

        public DynamicRestClientResponseException(HttpResponseMessage response) : base(response.ReasonPhrase)
        {
            Response = response;
        }
    }
}