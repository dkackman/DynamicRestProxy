using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        [TestCategory("integration")]
        [TestCategory("portable")]
        public async Task ExplicitGetInvoke()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                dynamic proxy = new HttpClientProxy(client);

                dynamic result = await proxy.metadata.mn.get();
                Assert.IsNotNull(result);
                Assert.IsTrue(result.name == "Minnesota");
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task GetMethodSegmentWithArgs()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                dynamic proxy = new HttpClientProxy(client);

                var result = await proxy.bills.mn("2013s1")("SF 1").get();
                Assert.IsNotNull(result);
                Assert.IsTrue(result.id == "MNB00017167");
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task GetMethod2PathAsProperty2Params()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                dynamic proxy = new HttpClientProxy(client);
                var parameters = new Dictionary<string, object>()
                {
                    { "lat", 44.926868 },
                    { "long", -93.214049 } // since long is a keyword we need to pass arguments in a Dictionary
                };
                var result = await proxy.legislators.geo.get(paramList: parameters);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task GetMethod1PathArg1Param()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                dynamic proxy = new HttpClientProxy(client);

                var result = await proxy.bills.get(state: "mn", chamber: "upper", status: "passed_upper");
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Count > 0);
                Assert.IsTrue(result[0].chamber == "upper");
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task EscapeParameterName()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://congress.api.sunlightfoundation.com");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                dynamic proxy = new HttpClientProxy(client);

                // this is the mechanism by which parameter names that are not valid c# property names can be used
                var parameters = new Dictionary<string, object>()
                {
                    { "chamber", "senate" },
                    { "history.house_passage_result", "pass" }
                };

                dynamic result = await proxy.bills.get(paramList: parameters);

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.results != null);
                Assert.IsTrue(result.results.Count > 0);

                foreach (dynamic bill in result.results)
                {
                    Assert.AreEqual("senate", (string)bill.chamber);
                    Assert.AreEqual("pass", (string)bill.history.house_passage_result);
                }
            }
        }
    }
}