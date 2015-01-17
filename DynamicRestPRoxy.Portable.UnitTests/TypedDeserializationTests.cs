using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/"))
            {
                dynamic bucketEndPoint = google.storage.v1.b("uspto-pair");

                dynamic dynamicBucket = await bucketEndPoint.get();
                Assert.IsNotNull(dynamicBucket);
                Assert.AreEqual(dynamicBucket.name, "uspto-pair");

                Bucket staticBucket = await bucketEndPoint.get(typeof(Bucket));
                Assert.IsNotNull(staticBucket);
                Assert.AreEqual(staticBucket.name, "uspto-pair");
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task DeserializeToString()
        {
            using (dynamic google = new DynamicRestClient("https://www.google.com/"))
            {
                string content = await google.get(typeof(string));

                Assert.IsFalse(string.IsNullOrEmpty(content));
                Assert.IsTrue(content.StartsWith("<!doctype html>"));
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task DeserializeToByteArray()
        {
            using (dynamic google = new DynamicRestClient("https://www.google.com/"))
            {
                byte[] content = await google.get(typeof(byte[]));

                Assert.IsNotNull(content);
                Assert.IsTrue(content.Length > 0);

                var s = Encoding.UTF8.GetString(content);
                Assert.IsFalse(string.IsNullOrEmpty(s));
                Assert.IsTrue(s.StartsWith("<!doctype html>"));
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task DeserializeToStream()
        {
            using (dynamic google = new DynamicRestClient("https://www.google.com/"))
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
    }
}
