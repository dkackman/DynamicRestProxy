using System;
using System.Net.Http;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Exception thrown when response status does not indicate success (<see cref="HttpResponseMessage.IsSuccessStatusCode"/>)
    /// Allows response content and headers to be inspected on failure
    /// </summary>
    public class DynamicRestClientResponseException : HttpRequestException
    {
        /// <summary>
        /// The response
        /// </summary>
        public HttpResponseMessage Response { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="response">The response</param>
        public DynamicRestClientResponseException(HttpResponseMessage response)
            : base(response?.ReasonPhrase) => Response = response;

        /// <summary>
        /// ctore
        /// </summary>
        /// <param name="response">The response</param>
        /// <param name="inner">An inner exception</param>
        public DynamicRestClientResponseException(HttpResponseMessage response, Exception inner)
            : base(response?.ReasonPhrase, inner) => Response = response;
    }
}