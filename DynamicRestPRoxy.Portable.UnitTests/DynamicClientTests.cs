using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class DynamicClientTests
    {
        [TestMethod]
        [TestCategory("integration")]
        [TestCategory("portable-client")]
        public async Task EscapeUriSegmentsUsingClient()
        {
            using (dynamic client = new DynamicRestClient("http://openstates.org/api/v1/"))
            {
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                var result = await client.bills.mn("2013s1")("SF 1").get(apikey: key);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.id == "MNB00017167");
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        [TestCategory("portable-client")]
        public async Task ExplicitGetInvokeUsingClient()
        {
            using (dynamic client = new DynamicRestClient("http://openstates.org/api/v1/"))
            {
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                dynamic result = await client.metadata.mn.get(apikey: key);

                Assert.IsNotNull(result);
                Assert.AreEqual("Minnesota", result.name);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task CoordinateFromPostalCode()
        {
            using (dynamic client = new DynamicRestClient("http://dev.virtualearth.net/REST/v1/"))
            {
                string key = CredentialStore.RetrieveObject("bing.key.json").Key;

                dynamic result = await client.Locations.get(postalCode: "55116", countryRegion: "US", key: key);

                Assert.AreEqual(200, result.statusCode);
                Assert.IsTrue(result.resourceSets.Count > 0);
                Assert.IsTrue(result.resourceSets[0].resources.Count > 0);

                var r = result.resourceSets[0].resources[0].point.coordinates;
                Assert.IsTrue((44.9108238220215).AboutEqual((double)r[0]));
                Assert.IsTrue((-93.1702041625977).AboutEqual((double)r[1]));
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task GetMethod2PathAsProperty2Params()
        {
            using (dynamic client = new DynamicRestClient("http://openstates.org/api/v1/"))
            {
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;

                var parameters = new Dictionary<string, object>()
            {
                { "lat", 44.926868 },
                { "long", -93.214049 } // since long is a keyword, pass arguments in a Dictionary
            };
                var result = await client.legislators.geo.get(paramList: parameters, apikey: key);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task ReservedWordNameEscapeWithCSharpSyntax()
        {
            using (dynamic client = new DynamicRestClient("http://openstates.org/api/v1/"))
            {
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;

                //escape the reserved word "long" with an @ symbol
                var result = await client.legislators.geo.get(apikey: key, lat: 44.926868, @long: -93.214049);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task ReservedWordNameEscapeWithDictionary()
        {
            using (dynamic client = new DynamicRestClient("http://congress.api.sunlightfoundation.com"))
            {
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;

                var parameters = new Dictionary<string, object>()
                {
                    { "chamber", "senate" },
                    { "history.house_passage_result", "pass" } // history.house_passage_result couldn't be used a C# parameter name                
                };

                dynamic result = await client.bills.get(paramList: parameters, apikey: key);

                foreach (dynamic bill in result.results)
                {
                    Assert.AreEqual("senate", bill.chamber);
                    Assert.AreEqual("pass", bill.history.house_passage_result);
                }
            }
        }
    }

}