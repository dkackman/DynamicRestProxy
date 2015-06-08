using System;
using System.IO;
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
        [Ignore] // drive scopes don't work with device pin based oauth2 - ignore for now
        public async Task UploadFile()
        {
            var auth = MockInitialization.GetAuthClient("email profile https://www.googleapis.com/auth/devstorage.read_write");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var defaults = new DynamicRestClientDefaults()
            {
                AuthScheme = "OAuth",
                AuthToken = _token
            };

            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", MockInitialization.Handler, false, defaults))
            {
                dynamic result = await google.upload.drive.v2.files.post(File.OpenRead(@"D:\temp\test.png"), uploadType: "media", title: "unit_test.jpg");

                Assert.IsNotNull(result);
            }
        }
    }
}