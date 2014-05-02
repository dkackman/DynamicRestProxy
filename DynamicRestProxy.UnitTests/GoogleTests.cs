using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;

using RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.UnitTests
{
    [TestClass]
    public class GoogleTests
    {
        private static string _token;

        private async Task Authenticate()
        {
            if (!string.IsNullOrEmpty(_token))
                return;

            if (CredentialStore.ObjectExists("google.auth.json"))
            {
                var access = CredentialStore.RetrieveObject("google.auth.json");

                if (DateTime.UtcNow >= DateTime.Parse((string)access.expiry))
                {
                    access = await RefreshAccessToken(access);
                    StoreAccess(access);
                }
                
                _token = access.access_token;
            }
            else
            {
                var access = await GetNewAccessToken();
                StoreAccess(access);
                _token = access.access_token;
            }
        }

        private static void StoreAccess(dynamic access)
        {
            access.expiry = DateTime.UtcNow.Add(TimeSpan.FromSeconds((int)access.expires_in));
            CredentialStore.StoreObject("google.auth.json", access);
        }

        private async Task<dynamic> RefreshAccessToken(dynamic access)
        {
            Assert.IsNotNull((string)access.refresh_token);

            dynamic key = CredentialStore.JsonKey("google").installed;

            dynamic proxy = new RestProxy("https://accounts.google.com");
            var response = await proxy.o.oauth2.token.post(client_id: key.client_id, client_secret: key.client_secret, refresh_token: access.refresh_token, grant_type: "refresh_token");
            Assert.IsNotNull(response);
            response.refresh_token = access.refresh_token; // the new access token doesn't have a new refresh token so move our current one here for long term storage
            return response;
        }

        private async Task<dynamic> GetNewAccessToken()
        {
            dynamic key = CredentialStore.JsonKey("google").installed;

            dynamic proxy = new RestProxy("https://accounts.google.com");
            var response = await proxy.o.oauth2.device.code.post(client_id: key.client_id, scope: "email profile https://www.googleapis.com/auth/calendar");
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

            dynamic tokenResonse = null;
            // we are using the device flow so enter the code in the browser - poll google for success
            while (time < expiration)
            {
                Thread.Sleep(interval * 1000);
                tokenResonse = await proxy.o.oauth2.token.post(client_id: key.client_id, client_secret: key.client_secret, code: response.device_code, grant_type: "http://oauth.net/grant_type/device/1.0");
                if (tokenResonse.access_token != null)
                    break;
                time += interval;
            }

            Assert.IsNotNull(tokenResonse);
            return tokenResonse;
        }

        [TestMethod]
        [TestCategory("integration")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetUserProfile()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var client = new RestClient("https://www.googleapis.com");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestProxy(client);
            var profile = await proxy.oauth2.v1.userinfo.get();

            Assert.IsNotNull(profile);
            Assert.AreEqual((string)profile.family_name, "Kackman");
        }

        [TestMethod]
        [TestCategory("integration")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetCalendarList()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestProxy(client);
            var list = await proxy.users.me.calendarList.get();

            Assert.IsNotNull(list);
            Assert.AreEqual((string)list.kind, "calendar#calendarList");
        }

        [TestMethod]
        [TestCategory("ordered")]
        // [Ignore] // - this test requires user interaction
        public async Task CreateCalendar()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestProxy(client);

            dynamic calendar = new ExpandoObject();
            calendar.summary = "unit_testing";

            var list = await proxy.calendars.post(calendar);

            Assert.IsNotNull(list);
            Assert.AreEqual((string)list.summary, "unit_testing");
        }

        [TestMethod]
        [TestCategory("ordered")]
        //  [Ignore] // - this test requires user interaction
        public async Task UpdateCalendar()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestProxy(client);
            var list = await proxy.users.me.calendarList.get();
            Assert.IsNotNull(list);

            string id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.id).FirstOrDefault();
            Assert.IsFalse(string.IsNullOrEmpty(id));

            var guid = Guid.NewGuid().ToString();
            dynamic calendar = new ExpandoObject();
            calendar.summary = "unit_testing";
            calendar.description = guid;

            var result = await proxy.calendars(id).put(calendar);
            Assert.IsNotNull(result);

            list = await proxy.users.me.calendarList.get();
            Assert.IsNotNull(list);
            string description = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.description).FirstOrDefault();

            Assert.AreEqual(guid, description);
        }

        [TestMethod]
        [TestCategory("ordered")]
        //  [Ignore] // - this test requires user interaction
        public async Task DeleteCalendar()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestProxy(client);
            var list = await proxy.users.me.calendarList.get();
            Assert.IsNotNull(list);

            string id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.id).FirstOrDefault();
            Assert.IsFalse(string.IsNullOrEmpty(id));

            var result = await proxy.calendars(id).delete();
            Assert.IsNull(result);

            list = await proxy.users.me.calendarList.get();
            Assert.IsNotNull(list);
            id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.id).FirstOrDefault();

            Assert.IsTrue(string.IsNullOrEmpty(id));
        }
    }
}
