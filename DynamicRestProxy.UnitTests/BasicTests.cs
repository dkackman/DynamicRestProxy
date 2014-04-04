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
        public async Task GetMethod1PathAsArg()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", APIKEY.Key);

            dynamic service = new Service(client);

            var result = await service.metadata("mn");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Minnesota");
        }

        [TestMethod]
        public async Task GetMethod3PathAsArg()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", APIKEY.Key);

            dynamic service = new Service(client);

            var result = await service.bills("mn", "2013s1", "SF 1");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.id == "MNB00017167");
        }

        [TestMethod]
        public async Task GetMethod2PathAsProperty2Params()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", APIKEY.Key);

            dynamic service = new Service(client);

            var result = await service.legislators.geo(lat: 44.926868, lon: -93.214049);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 0);
        }

        [TestMethod]
        public async Task GetMethod1PathArg1Param()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", APIKEY.Key);

            dynamic service = new Service(client);

            var result = await service.bills(state: "mn", chamber: "upper", status: "passed_upper");
            Assert.IsNotNull(result);
        }
    }
}
