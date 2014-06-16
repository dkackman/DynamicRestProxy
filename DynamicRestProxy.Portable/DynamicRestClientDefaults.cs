using System.Collections.Generic;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Default values that will be added all all requests
    /// </summary>
    public class DynamicRestClientDefaults
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DynamicRestClientDefaults()
        {
            DefaultParameters = new Dictionary<string, object>();
            DefaultHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Default paramter values
        /// </summary>
        public IDictionary<string, object> DefaultParameters { get; private set; }

        /// <summary>
        /// Default request header values
        /// </summary>
        public IDictionary<string, string> DefaultHeaders { get; private set; }

        /// <summary>
        /// OAuth token to add to all requests
        /// </summary>
        public string OAuthToken { get; set; }
    }
}
