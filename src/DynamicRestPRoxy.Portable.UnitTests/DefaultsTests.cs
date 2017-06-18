using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class DefaultsTests
    {
        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UserSuppliedAcceptHeaderOverridesDefaultAcceptHeaders()
        {
            var defaults = new DynamicRestClientDefaults();
            defaults.DefaultHeaders.Add("AccEpt", "application/json");
            
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", MockInitialization.Handler, false, defaults))
            using (HttpResponseMessage response = await google.storage.v1.b("uspto-pair").get(typeof(HttpResponseMessage)))
            {
                response.EnsureSuccessStatusCode();
                Assert.IsNotNull(response.RequestMessage.Headers.Accept);
                Assert.AreEqual(1, response.RequestMessage.Headers.Accept.Count);
                Assert.AreEqual("application/json", response.RequestMessage.Headers.Accept.First().MediaType);
            }
        }
    }
}
