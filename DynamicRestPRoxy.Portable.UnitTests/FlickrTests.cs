using System;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class FlickrTests
    {
        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task FindUserByName()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://api.flickr.com/services/rest/");

                dynamic proxy = new HttpClientProxy(client);

                var key = CredentialStore.JsonKey("flickr");
                
                dynamic result = await proxy.get(method: "flickr.people.findByUsername", username: "dkackman", format: "json", api_key: key.Key, nojsoncallback: "1");
                Assert.IsNotNull(result);
                Assert.AreEqual("9039518@N03", (string)result.user.id);
            }
        }

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        [Ignore]
        public  void UploadPhoto()
        {
            var key = CredentialStore.JsonKey("flickr");

            //var client = new RestClient("https://up.flickr.com/services/");
            //client.AddDefaultParameter("format", "json");
            //client.AddDefaultParameter("api_key", (string)key.Key);
            //client.AddDefaultParameter("nojsoncallback", "1");

            //var file = new FileInfo(@"D:\temp\test.png");

            //dynamic proxy = new HttpClientProxy(client);

            //dynamic result = await proxy.upload.post(photo: file);

            //Assert.IsNotNull(result);

        }
    }
}
