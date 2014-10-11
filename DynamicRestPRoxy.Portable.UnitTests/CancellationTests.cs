using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class CancellationTests
    {
        [TestMethod]
        public void Cancel()
        {
            dynamic client = new DynamicRestClient("http://dev.virtualearth.net/REST/v1/");

            string key = CredentialStore.RetrieveObject("bing.key.json").Key;

            using (var source = new CancellationTokenSource())
            {
                // run request on a different thread and do not await thread
                Task t = client.Locations.get(source.Token, postalCode: "55116", countryRegion: "US", key: key);
                
                // cancel on unit test thread
                source.Cancel();

                try
                {
                    // this will throw
                    Task.WaitAll(t);
                    Assert.Fail("Task was not cancelled");
                }
                catch (AggregateException e)
                {
                    Assert.IsTrue(e.InnerExceptions.OfType<TaskCanceledException>().Any());
                }
            }
        }
    }
}
