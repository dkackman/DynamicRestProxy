using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class GoogleTests
    {
        private static string _token = null;

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetUserProfile()
        {
            var auth = new GoogleOAuth2("email profile");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _token);

                dynamic proxy = new HttpClientProxy(client);
                var profile = await proxy.oauth2.v1.userinfo.get();

                Assert.IsNotNull(profile);
                Assert.AreEqual("Kackman", profile.family_name);
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetCalendarList()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/calendar/v3/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _token);

                dynamic proxy = new HttpClientProxy(client);
                var list = await proxy.users.me.calendarList.get();

                Assert.IsNotNull(list);
                Assert.AreEqual("calendar#calendarList", list.kind);
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("ordered")]
        // [Ignore] // - this test requires user interaction
        public async Task CreateCalendar()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/calendar/v3/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _token);

                dynamic proxy = new HttpClientProxy(client);

                dynamic calendar = new ExpandoObject();
                calendar.summary = "unit_testing";

                var list = await proxy.calendars.post(calendar);

                Assert.IsNotNull(list);
                Assert.AreEqual("unit_testing", list.summary);
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("ordered")]
        //  [Ignore] // - this test requires user interaction
        public async Task UpdateCalendar()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/calendar/v3/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _token);
                dynamic proxy = new HttpClientProxy(client);
                var list = await proxy.users.me.calendarList.get();
                Assert.IsNotNull(list);

                string id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => cal.id).FirstOrDefault();
                Assert.IsFalse(string.IsNullOrEmpty(id));

                var guid = Guid.NewGuid().ToString();
                dynamic calendar = new ExpandoObject();
                calendar.summary = "unit_testing";
                calendar.description = guid;

                var result = await proxy.calendars(id).put(calendar);
                Assert.IsNotNull(result);

                list = await proxy.users.me.calendarList.get();
                Assert.IsNotNull(list);
                string description = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => cal.description).FirstOrDefault();

                Assert.AreEqual(guid, description);
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("ordered")]
        //  [Ignore] // - this test requires user interaction
        public async Task DeleteCalendar()
        {
            var auth = new GoogleOAuth2("email profile https://www.googleapis.com/auth/calendar");
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/calendar/v3/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _token);

                dynamic proxy = new HttpClientProxy(client);
                var list = await proxy.users.me.calendarList.get();
                Assert.IsNotNull(list);

                string id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => cal.id).FirstOrDefault();
                Assert.IsFalse(string.IsNullOrEmpty(id), "calendar does not exist so we can't delete it");

                var result = await proxy.calendars(id).delete();
                Assert.IsNull(result);

                //var list2 = await proxy.users.me.calendarList.get();
                //Assert.IsNotNull(list2);
                //var id2 = ((IEnumerable<dynamic>)(list2.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.id).FirstOrDefault();

                //Assert.IsTrue(string.IsNullOrEmpty(id2), "calendar seems to have not been deleted");
            }
        }
    }
}