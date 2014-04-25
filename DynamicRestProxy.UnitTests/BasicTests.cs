using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

using RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.UnitTests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        [TestCategory("online")]
        public async Task ExplicitGetInvoke()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic proxy = new RestProxy(client);

            dynamic result = await proxy.metadata.mn.get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Minnesota");
        }

        [TestMethod]
        [TestCategory("online")]
        public async Task GetMethodSegmentWithArgs()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic proxy = new RestProxy(client);

            var result = await proxy.bills.mn.segment("2013s1").segment("SF 1").get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.id == "MNB00017167");
        }

        [TestMethod]
        [TestCategory("online")]
        public async Task EscapeInvalidSegmentStartCharacter()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new RestProxy(client);

            var result = await service.bills.mn._2013s1.segment("SF 1").get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.id == "MNB00017167");
        }

        [TestMethod]
        [TestCategory("online")]
        public async Task GetMethod2PathAsProperty2Params()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic proxy = new RestProxy(client);

            var result = await proxy.legislators.geo.get(lat: 44.926868, _long: -93.214049);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        [TestCategory("online")]
        public async Task GetMethod1PathArg1Param()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic proxy = new RestProxy(client);

            var result = await proxy.bills.get(state: "mn", chamber: "upper", status: "passed_upper");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Count > 0);
            Assert.IsTrue(result[0].chamber == "upper");
        }

        [TestMethod]
        [TestCategory("online")]
        public async Task EscapeParameterName()
        {
            var client = new RestClient("http://congress.api.sunlightfoundation.com");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic proxy = new RestProxy(client);

            // this is the mechanism by which parameter names that are not valid c# property names can be used
            var parameters = new Dictionary<string, object>();
            parameters.Add("chamber", "senate");
            parameters.Add("history.house_passage_result", "pass");

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
