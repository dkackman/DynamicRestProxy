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
    static class Extensions
    {
        public static bool AboutEqual(this double x, double y)
        {
            double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }
    }

    [TestClass]
    public class BingMapsTests
    {
        [TestMethod]
        [TestCategory("portable")]
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

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task GetFormattedAddressFromCoordinate()
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
                var result = await proxy.Locations("44.9108238220215,-93.1702041625977").get(includeEntityTypes: "Address,PopulatedPlace,Postcode1,AdminDivision1,CountryRegion", key: key);

                Assert.AreEqual((int)result.statusCode, 200);
                Assert.IsTrue(result.resourceSets.Count > 0);
                Assert.IsTrue(result.resourceSets[0].resources.Count > 0);

                var address = result.resourceSets[0].resources[0].address.formattedAddress;
                Assert.AreEqual((string)address, "1012 Davern St, St Paul, MN 55116");
            }
        }
    }
}