using System.Collections.Generic;

namespace DynamicRestProxy.PortableHttpClient
{
    public class DynamicRestClientDefaults
    {
        public DynamicRestClientDefaults()
        {
            DefaultParameters = new Dictionary<string, object>();
            DefaultHeaders = new Dictionary<string, string>();
        }

        public IDictionary<string, object> DefaultParameters { get; private set; }

        public IDictionary<string, string> DefaultHeaders { get; private set; }

        public string OAuthToken { get; set; }
    }
}
