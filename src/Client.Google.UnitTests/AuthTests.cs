using System.Dynamic;
using System.Threading.Tasks;
using System.Net.Http.Headers;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Client.Google.UnitTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class AuthTests
    {
        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task AuthenticateAndGetUserName()
        {
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", MockInitialization.Handler, false, null, 
                async (request, cancelToken) =>
                {
                    // this demonstrates how to use the configuration callback to handle authentication 
                    var auth = MockInitialization.GetAuthClient("email profile");
                    var token = await auth.Authenticate("", cancelToken);
                    Assert.IsNotNull(token, "auth failed");

                    request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);
                }))
            {
                var profile = await google.oauth2.v1.userinfo.get();

                Assert.IsNotNull(profile);
                Assert.AreEqual("Kackman", profile.family_name);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task CreateGoogleCalendarUsingClient()
        {
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/calendar/v3/", MockInitialization.Handler, false, null, 
                async (request, cancelToken) =>
                {
                    // this demonstrates how to use the configuration callback to handle authentication 
                    var auth = MockInitialization.GetAuthClient("email profile https://www.googleapis.com/auth/calendar");
                    var token = await auth.Authenticate("", cancelToken);
                    Assert.IsNotNull(token, "auth failed");

                    request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);
                }))
            {
                dynamic calendar = new ExpandoObject();
                calendar.summary = "unit_testing";

                var list = await google.calendars.post(calendar);

                Assert.IsNotNull(list);
                Assert.AreEqual(list.summary, "unit_testing");
            }
        }
    }
}