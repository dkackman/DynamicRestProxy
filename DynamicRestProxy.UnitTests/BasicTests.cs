using System;
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
        public async Task ExplicitGetInvoke()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new RestProxy(client);

            dynamic result = await service.metadata.mn.get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Minnesota");
        }

        [TestMethod]
        public async Task GetMethodSegmentWithArgs()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new RestProxy(client);

            var result = await service.bills.mn.segment("2013s1").segment("SF 1").get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.id == "MNB00017167");
        }

        [TestMethod]
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
        public async Task GetMethod2PathAsProperty2Params()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new RestProxy(client);

            var result = await service.legislators.geo.get(lat: 44.926868, _long: -93.214049);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task GetMethod1PathArg1Param()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new RestProxy(client);

            var result = await service.bills.get(state: "mn", chamber: "upper", status: "passed_upper");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Count > 0);
            Assert.IsTrue(result[0].chamber == "upper");
        }
    }
}
