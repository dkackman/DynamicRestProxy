using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class CallbackTests
    {
        public async Task BasicCallback()
        {
            dynamic client = new DynamicRestClient("http://dev.virtualearth.net/REST/v1/",
               configureRequest: (request, token) =>
               {
                   Debug.WriteLine(request.RequestUri);
                   return Task.CompletedTask;
               });

            dynamic result = await client.Locations.get(postalCode: "55116", countryRegion: "US", key: "key");
        }
    }
}
