﻿using System;
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
    [DeploymentItem(@"MockResponses\")]
    public class BasicTests
    {
        [TestMethod]
        [TestCategory("integration")]
        [TestCategory("portable")]
        public async Task ExplicitGetInvoke()
        {
            using (var client = new HttpClient(MockInitialization.Handler, false))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                using (dynamic proxy = new DynamicRestClient(client))
                {
                    dynamic result = await proxy.metadata.mn.get();
                    Assert.IsNotNull(result);
                    Assert.AreEqual("Minnesota", result.name);
                }
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task GetMethodSegmentWithArgs()
        {
            using (var client = new HttpClient(MockInitialization.Handler, false))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                using (dynamic proxy = new DynamicRestClient(client))
                {
                    var result = await proxy.bills.mn("2013s1")("SF 1").get();
                    Assert.IsNotNull(result);
                    Assert.AreEqual("MNB00017167", result.id);
                }
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task GetMethod2PathAsProperty2Params()
        {
            using (var client = new HttpClient(MockInitialization.Handler, false))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                using (dynamic proxy = new DynamicRestClient(client))
                {
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
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task GetMethod1PathArg1Param()
        {
            using (var client = new HttpClient(MockInitialization.Handler, false))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                using (dynamic proxy = new DynamicRestClient(client))
                {
                    var result = await proxy.bills.get(state: "mn", chamber: "upper", status: "passed_upper");
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result.Count > 0);
                    Assert.AreEqual("upper", (string)result[0].chamber);
                }
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task EscapeParameterName()
        {
            using (var client = new HttpClient(MockInitialization.Handler, false))
            {
                client.BaseAddress = new Uri("http://congress.api.sunlightfoundation.com");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                using (dynamic proxy = new DynamicRestClient(client))
                {
                    // this is the mechanism by which parameter names that are not valid c# property names can be used
                    var parameters = new Dictionary<string, object>()
                {
                    { "chamber", "senate" },
                    { "history.house_passage_result", "pass" }
                };

                    dynamic result = await proxy.bills.get(paramList: parameters);

                    Assert.IsNotNull(result);
                    Assert.IsNotNull(result.results);
                    Assert.IsTrue(result.results.Count > 0);

                    foreach (dynamic bill in result.results)
                    {
                        Assert.AreEqual("senate", bill.chamber);
                        Assert.AreEqual("pass", bill.history.house_passage_result);
                    }
                }
            }
        }
    }
}