using System;
using System.Dynamic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class DynamicClientTests
    {
        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task CoordinateFromPostalCode()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://dev.virtualearth.net/REST/v1/");

                string key = CredentialStore.Key("bing");

                dynamic proxy = new HttpClientProxy(client);
                var result = await proxy.Locations.get(postalCode: "55116", countryRegion: "US", key: key);

                Assert.AreEqual((int)result.statusCode, 200);
                Assert.IsTrue(result.resourceSets.Count > 0);
                Assert.IsTrue(result.resourceSets[0].resources.Count > 0);

                var r = result.resourceSets[0].resources[0].point.coordinates;
                Assert.IsTrue((44.9108238220215).AboutEqual((double)r[0]));
                Assert.IsTrue((-93.1702041625977).AboutEqual((double)r[1]));
            }
        }
    }
}
