using System;
using System.Dynamic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace Client.Google.UnitTests
{
    [TestClass]
    public class DriveTests
    {
        private static string _token = null;

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadFile()
        {
            _token = await GoogleOAuth2.Authenticate(_token);
            Assert.IsNotNull(_token);

            throw new NotImplementedException();
        }
    }
}
