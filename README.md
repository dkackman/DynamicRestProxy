DynamicRestProxy
================

A rest client using the .NET Dynamic Language Runtime

This is a set of classes that wrap the [RestSharp RestClient](http://restsharp.org/) with a [DyamicObject](http://msdn.microsoft.com/en-us/library/system.dynamic.dynamicobject(v=vs.110).aspx). The wrapper translates dyanmic method invlocations and call paths into REST requests. 

All requests are asynynchronous and return dyanmic objects.

The intent is to make it easier to access REST API's from C# without needing to create strongly typed API wrappers and numerous static POCO types for basic DTO responses. 

Is currently a work in progress. Supports the GET, POST, and DELETE verbs.

Some examples:

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
