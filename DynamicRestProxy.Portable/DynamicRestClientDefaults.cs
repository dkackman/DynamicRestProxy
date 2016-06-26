using System;
using System.Collections.Generic;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Default values that will be added to all requests
    /// </summary>
    public sealed class DynamicRestClientDefaults
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DynamicRestClientDefaults()
        {
            DefaultParameters = new Dictionary<string, object>();
            DefaultHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Default parameter values
        /// </summary>
        public IDictionary<string, object> DefaultParameters { get; private set; }

        /// <summary>
        /// Default request header values
        /// </summary>
        public IDictionary<string, string> DefaultHeaders { get; private set; }

        /// <summary>
        /// Auth token to add to all requests
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// The Auth scheme used for AuthToken
        /// </summary>
        public string AuthScheme { get; set; }

        /// <summary>
        /// User agent string in the format product/version
        /// </summary>
        public string UserAgent { get; set; }
    }
}
