using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;

using RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.RestSharp.UnitTests
{
    [TestClass]
    public class GoogleTests
    {
        private static string _token = null;

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetUserProfile()
        {
            var auth = new GoogleOAuth2("profile");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var client = new RestClient("https://www.googleapis.com");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestSharpProxy(client);
            var profile = await proxy.oauth2.v1.userinfo.get();

            Assert.IsNotNull(profile);
            Assert.AreEqual((string)profile.family_name, "Kackman");
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetCalendarList()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestSharpProxy(client);
            var list = await proxy.users.me.calendarList.get();

            Assert.IsNotNull(list);
            Assert.AreEqual((string)list.kind, "calendar#calendarList");
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("ordered")]
        // [Ignore] // - this test requires user interaction
        public async Task CreateCalendar()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestSharpProxy(client);

            dynamic calendar = new ExpandoObject();
            calendar.summary = "unit_testing";

            var list = await proxy.calendars.post(calendar);

            Assert.IsNotNull(list);
            Assert.AreEqual((string)list.summary, "unit_testing");
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("ordered")]
        //  [Ignore] // - this test requires user interaction
        public async Task UpdateCalendar()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestSharpProxy(client);
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
        [TestCategory("RestSharp")]
        [TestCategory("ordered")]
        //  [Ignore] // - this test requires user interaction
        public async Task DeleteCalendar()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var client = new RestClient("https://www.googleapis.com/calendar/v3");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic proxy = new RestSharpProxy(client);
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
