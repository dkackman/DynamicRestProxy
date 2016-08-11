using System;
using System.Threading.Tasks;

using DynamicRestProxy.PortableHttpClient;

using Xunit;

namespace CrossPlatformTests
{
    public class CoreTests
    {
        [Fact]
        public async Task SimpleGet() 
        {   
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/"))
            {
                dynamic bucket = google.storage.v1.b("uspto-pair");

                dynamic metaData = await bucket.get();
                
                Assert.NotNull(metaData);

                dynamic objects = await bucket.o.get();
                Assert.NotNull(objects);
                Assert.True(objects.items.Count > 0);
            }
        }
    }
}
