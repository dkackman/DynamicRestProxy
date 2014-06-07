DynamicRestProxy
================

A rest client proxy using the .NET [Dynamic Language Runtime](http://msdn.microsoft.com/en-us/library/dd233052(v=vs.110).aspx). 


This is a set of classes that wrap a concrete implementation of http client communication with a [DyamicObject](http://msdn.microsoft.com/en-us/library/system.dynamic.dynamicobject(v=vs.110).aspx). The wrapper translates dyanmic method invocations and endpoint paths into REST requests. 

All requests are asynynchronous and return dyanmic objects.

The intent is to make it easier to access REST API's from C# without needing to create strongly typed API wrappers and numerous static POCO types for basic DTO responses. 

Is currently a work in progress. Supports the GET, POST, PUT, and DELETE verbs.

Current concrete implementations include 
- [RestSharp RestClient](http://restsharp.org/) 
- [Portable HttpClient](https://www.nuget.org/packages/Microsoft.Net.Http/) 

If you try to run the unit tests take a close look at the CredentialStore class in the unit test project. It's pretty straighforward and you can use it to supply your own api keys while keeping them out of the code.

Some RestSharp examples:

Basic GET with no parameters:
http://openstates.org/api/v1/metadata/mn/

            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new RestProxy(client);

            dynamic result = await service.metadata.mn.get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Minnesota");
            
            
Access an endpoint with URL parameters:
http://openstates.org/api/v1/legislators/geo/?lat=44.926868&long=-93.214049

            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new DynamicRestClient(client);

            var result = await service.legislators.geo(lat: 44.926868, _long: -93.214049);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            
A Bing Maps Geocode endpoint:
http://dev.virtualearth.net/REST/v1/Locations?postalCode=55116&countryRegion=US&key=...

            var client = new RestClient("http://dev.virtualearth.net/REST/v1/");
            client.AddDefaultParameter("key", CredentialStore.Key("bing"));

            dynamic service = new RestProxy(client);
            
            var result = await service.Locations.get(postalCode: "55116", countryRegion: "US");

            Assert.AreEqual((int)result.statusCode, 200);
            Assert.IsTrue(result.resourceSets.Count > 0);
            Assert.IsTrue(result.resourceSets[0].resources.Count > 0);

            var r = result.resourceSets[0].resources[0].point.coordinates;
            Assert.IsTrue((44.9108238220215).AboutEqual((double)r[0]));
            Assert.IsTrue((-93.1702041625977).AboutEqual((double)r[1]));
            
Create a Google calendar using POST:

            var api = new RestClient("https://www.googleapis.com/calendar/v3");
            api.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic apiProxy = new RestProxy(api);

            dynamic calendar = new ExpandoObject();
            calendar.summary = "unit_testing";

            var response = await apiProxy.calendars.post(calendar);

            Assert.IsNotNull(response);
            Assert.AreEqual((string)response.summary, "unit_testing");
            
Update the calendar using PUT:

            var guid = Guid.NewGuid().ToString();
            dynamic updated = new ExpandoObject();
            updated.summary = "unit_testing";
            updated.description = guid;
            var result = await apiProxy.calendars(response.id).put(updated);
            Assert.IsNotNull(result);
            Assert.AreEqual(guid, (string)result.description);

Delete the calendar:

            await apiProxy.calendars(response.id).delete();

