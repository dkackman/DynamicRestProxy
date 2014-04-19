using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.UnitTests
{
    [TestClass]
    public class GoogleTests
    {
        private static dynamic GetGoogleKey()
        {
            return APIKEY.JsonKey("google").installed;
        }

        [TestMethod]
        [Ignore] // - this test requires user interaction
        public async Task DeviceAuthentication()
        {
            dynamic key = GetGoogleKey();

            var client = new RestClient("https://accounts.google.com");
            dynamic proxy = new RestProxy(client);
            var response = await proxy.o.oauth2.device.code.post(client_id: key.client_id, scope: "email profile");
            Assert.IsNotNull(response);

            Debug.WriteLine((string)response.user_code);

            // use clip.exe to put the user code on the clipboard
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = string.Format("/c echo {0} | clip", response.user_code);
            p.Start();

            // this requires user permission - open a broswer - enter the user_code which is now in the clipboard
            Process.Start((string)response.verification_url);

            int expiration = response.expires_in;
            int interval = response.interval;
            int time = interval;

            // we are using the device flow so enter the code in the browser - poll google for success
            string token = null;
            while (time < expiration && string.IsNullOrEmpty(token))
            {
                Thread.Sleep(interval * 1000);
                var tokenResonse = await proxy.o.oauth2.token.post(client_id: key.client_id, client_secret: key.client_secret, code:response.device_code, grant_type:"http://oauth.net/grant_type/device/1.0");
                token = tokenResonse.access_token;
                time += interval;
            }

            Assert.IsNotNull(token);

            var api = new RestClient("https://www.googleapis.com");
            api.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token);
            dynamic apiProxy = new RestProxy(api);
            var profile = await apiProxy.oauth2.v1.userinfo();

            Assert.IsNotNull(profile);
            Assert.AreEqual((string)profile.family_name, "Kackman");
        }
    }
}
