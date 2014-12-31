using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DynamicRestProxy.PortableHttpClient;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    public class Bucket
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string selfLink { get; set; }
        public string name { get; set; }
        public DateTime timeCreated { get; set; }
        public int metageneration { get; set; }
        public string location { get; set; }
        public string storageClass { get; set; }
        public string etag { get; set; }
    }

    [TestClass]
    public class TypedDeserializationTests
    {
#if EXPERIMENTAL_GENERICS

        [TestMethod]
        [TestCategory("integration")]
        [TestCategory("portable")]
        public async Task DeserializeToGenericType()
        {
            dynamic google = new DynamicRestClient("https://www.googleapis.com/");
            dynamic bucket = google.storage.v1.b("uspto-pair");

            Bucket metaData = await bucket.get<Bucket>();
            Assert.IsNotNull(metaData);
            Assert.AreEqual(metaData.name, "uspto-pair");
        }
#endif

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task DeserializeToStaticType()
        {
            dynamic google = new DynamicRestClient("https://www.googleapis.com/");
            dynamic bucketEndPoint = google.storage.v1.b("uspto-pair");

            dynamic dynamicBucket = await bucketEndPoint.get();
            Assert.IsNotNull(dynamicBucket);
            Assert.AreEqual(dynamicBucket.name, "uspto-pair");

            Bucket staticBucket = await bucketEndPoint.get(typeof(Bucket));
            Assert.IsNotNull(staticBucket);
            Assert.AreEqual(staticBucket.name, "uspto-pair");
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task DeserializeToString()
        {
            dynamic google = new DynamicRestClient("https://www.google.com/");
            string content = await google.get(typeof(string));

            Assert.IsFalse(string.IsNullOrEmpty(content));
            Assert.IsTrue(content.StartsWith("<!doctype html>"));
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task DeserializeToByteArray()
        {
            dynamic google = new DynamicRestClient("https://www.google.com/");
            byte[] content = await google.get(typeof(byte[]));

            Assert.IsNotNull(content);
            Assert.IsTrue(content.Length > 0);

            var s = Encoding.UTF8.GetString(content);
            Assert.IsFalse(string.IsNullOrEmpty(s));
            Assert.IsTrue(s.StartsWith("<!doctype html>"));
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task DeserializeToStream()
        {
            dynamic google = new DynamicRestClient("https://www.google.com/");
            using (Stream content = await google.get(typeof(Stream)))
            {
                Assert.IsNotNull(content);
                using (var reader = new StreamReader(content))
                {
                    var s = reader.ReadToEnd();
                    Assert.IsFalse(string.IsNullOrEmpty(s));
                    Assert.IsTrue(s.StartsWith("<!doctype html>"));
                }
            }
        }

        [TestMethod]
        public async Task DebugStreamedContent()
        {
            Stream stream = null; // in real life the consumer of the stream is far away 
            var client = new HttpClient();

            client.BaseAddress = new Uri("https://www.google.com/", UriKind.Absolute);
            using (var request = new HttpRequestMessage(HttpMethod.Get, "/"))
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                //here I would return the stream 
                stream = await response.Content.ReadAsStreamAsync();
            }

            Assert.IsNotNull(stream); // if response is disposed so is the stream
            Assert.IsTrue(stream.CanRead);
        }
    }
}
