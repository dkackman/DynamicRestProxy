﻿using System.Linq;
using System.IO;
using System.Dynamic;
using System.Threading.Tasks;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;
using System.Net;

namespace Client.Google.UnitTests
{
    [TestClass]
    public class CloudStorageTests
    {
        private static string _token = null;

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task GetNonJsonContent()
        {
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", MockInitialization.Handler))
            {
                dynamic zip = google.storage.v1.b("uspto-pair").o("applications%2F05900002.zip");

                byte[] zipBytes = await zip.get(typeof(byte[]), generation: 1370956749027000, alt: "media");

                Assert.IsNotNull(zipBytes);
                Assert.IsTrue(zipBytes.Any());
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task GetPublicBucket()
        {
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", MockInitialization.Handler))
            {
                dynamic bucket = google.storage.v1.b("uspto-pair");

                dynamic metaData = await bucket.get();
                Assert.IsNotNull(metaData);

                dynamic objects = await bucket.o.get();
                Assert.IsNotNull(objects);
                Assert.IsTrue(objects.items.Count > 0);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadObject()
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
            using (var stream = new StreamInfo(File.OpenRead(@"D:\temp\test.png"), "image/png"))
            {
                dynamic result = await google.upload.storage.v1.b.unit_tests.o.post(stream, name: new PostUrlParam("test_object"), uploadType: new PostUrlParam("media"));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task MultiPartUploadObject()
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
            using (var stream = new StreamInfo(File.OpenRead(@"D:\temp\test2.png"), "image/png"))
            {
                dynamic metaData = new ExpandoObject();
                metaData.name = "test2";
                dynamic result = await google.upload.storage.v1.b.unit_tests.o.post(metaData, stream, uploadType: new PostUrlParam("multipart"));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadString()
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
                dynamic result = await google.upload.storage.v1.b.unit_tests.o.post("text", name: new PostUrlParam("string_object"), uploadType: new PostUrlParam("media"));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadInt()
        {
            var auth = MockInitialization.GetAuthClient("email profile https://www.googleapis.com/auth/devstorage.read_write");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var defaults = new DynamicRestClientDefaults()
            {
                AuthScheme = "OAuth",
                AuthToken = _token
            };

            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults))
            {
                dynamic result = await google.upload.storage.v1.b.unit_tests.o.post(42, name: new PostUrlParam("int_object"), uploadType: new PostUrlParam("media"));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task GetBadUriThrowsResponseException()
        {
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", MockInitialization.Handler))
            {
                dynamic bucket = google.storage.v1.b("bad-uri");

                try
                {
                    dynamic metaData = await bucket.get();
                }
                catch (DynamicRestClientResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                    Assert.AreEqual("Not Found", ex.Response.ReasonPhrase);
                    Assert.IsFalse(ex.Response.IsSuccessStatusCode);
                }
            }
        }
    }
}