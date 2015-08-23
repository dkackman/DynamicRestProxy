using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

using DynamicRestProxy.PortableHttpClient;

namespace Universal.UnitTests
{
    [TestClass]
    public class BasicFunctionalityTests
    {
        [TestMethod]
        [TestCategory("Universal")]
        public async Task CanGetStorageFile()
        {

        }

        [TestMethod]
        [TestCategory("Universal")]
        public async Task GetPublicBucket()
        {
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/"))
            {
                dynamic bucket = google.storage.v1.b("uspto-pair");

                dynamic metaData = await bucket.get();
                Assert.IsNotNull(metaData);

                dynamic objects = await bucket.o.get();
                Assert.IsNotNull(objects);
                Assert.IsTrue(objects.items.Count > 0);
            }
        }
    }
}
