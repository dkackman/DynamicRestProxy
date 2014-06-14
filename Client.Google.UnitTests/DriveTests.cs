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

        private const string _scope = "email profile https://www.googleapis.com/auth/drive https://www.googleapis.com/auth/drive.file https://www.googleapis.com/auth/drive.appdata https://www.googleapis.com/auth/drive.apps.readonly";

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadFile()
        {
            var auth = new GoogleOAuth2(_scope);
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var defaults = new DynamicRestClientDefaults();
            defaults.OAuthToken = _token;

            dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults);

            dynamic result = await google.upload.drive.v2.files.post(File.OpenRead(@"D:\temp\test.png"), uploadType: "media", title: "unit_test.jpg");

            Assert.IsNotNull(result);
        }
    }
}
