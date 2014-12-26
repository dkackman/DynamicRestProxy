using System;
using System.Threading.Tasks;

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
    public class TypedDeserilzationTests
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
        [TestCategory("portable")]
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
    }
}
