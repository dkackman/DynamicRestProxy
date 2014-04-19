DynamicRestProxy
================

A rest client using the .NET Dynamic Language Runtime

This is a set of classes that wrap the [RestSharp RestClient](http://restsharp.org/) with a [DyamicObject](http://msdn.microsoft.com/en-us/library/system.dynamic.dynamicobject(v=vs.110).aspx). The wrapper translates dyanmic method invlocations and call paths into REST requests. 

All requests are asynynchronous and return dyanmic objects.

The intent is to make it easier to access REST API's from C# without needing to create strongly typed API wrappers and numerous static POCO types for basic DTO responses.

Some examples:

Get meta-data about Minnesota from SunLight Labs
http://openstates.org/api/v1/metadata/mn/

            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", CredentialStore.Key("sunlight"));

            dynamic service = new RestProxy(client);

            dynamic result = await service.metadata.mn.get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Minnesota");
            
            
Or to access an endpoint with URL parameters:
http://openstates.org/api/v1/legislators/geo/?lat=44.926868&long=-93.214049

            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", APIKEY.Key);

            dynamic service = new DynamicRestClient(client);

            var result = await service.legislators.geo(lat: 44.926868, _long: -93.214049);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            
Or to access a Bing Maps Geocode endpoint:

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
            
The end result is intended to be a coventions based way to interact with REST services, quickly and in a natural manner. 

Is currentlya work in progress. Supports the GET, POST, and DELETE verbs.
