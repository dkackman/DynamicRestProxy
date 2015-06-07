using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class CancellationTests
    {
        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]

        public void Cancel()
        {
            string key = CredentialStore.RetrieveObject("bing.key.json").Key;

            using (dynamic client = new DynamicRestClient("http://dev.virtualearth.net/REST/v1/", MockInitialization.Handler))
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

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]

        public void CancelPassesToConfigurationCallback()
        {
            using (var source = new CancellationTokenSource())
            {
                // the cancellation token here is the one we passed in below
                using (dynamic client = new DynamicRestClient("https://www.googleapis.com/oauth2/v1/userinfo", MockInitialization.Handler, false, null, 
                    async (request, cancelToken) =>
                    {
                        Assert.AreEqual(source.Token, cancelToken);

                        var oauth = new GoogleOAuth2("email profile");
                        var authToken = await oauth.Authenticate("", cancelToken);
                        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authToken);
                    }))
                {
                    // run request on a different thread and do not await thread
                    Task t = client.oauth2.v1.userinfo.get(source.Token);

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
}