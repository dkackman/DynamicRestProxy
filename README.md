DynamicRestProxy
================

A rest client using the .NET Dynamic Language Runtime

This is a set of classes that wrap the [RestSharp RestClient](http://restsharp.org/) with a [DyamicObject](http://msdn.microsoft.com/en-us/library/system.dynamic.dynamicobject(v=vs.110).aspx). The wrapper translates dyanmic method invlocations and call paths into REST requests. All requests are asynynchronous and return dyanmic objects.

The intent is to make it easier to access REST API's from C# without needing to create strongly typed API wrappers and numerous static POCO types for basic DTO responses.

Some examples:

In order to access this specific SunLine Labs endpoint and inspect the result as a dyanmic object.
http://openstates.org/api/v1/bills/mn/2013s1/SF%201/


            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", APIKEY.Key);

            dynamic service = new DynamicRestClient(client);

            var result = await service.bills("mn", "2013s1", "SF 1");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.id == "MNB00017167");
            
Or to access an endpoint with URL parameters:
http://openstates.org/api/v1/legislators/geo/?lat=44.926868&long=-93.214049

            var client = new RestClient("http://openstates.org/api/v1");
            client.AddDefaultHeader("X-APIKEY", APIKEY.Key);

            dynamic service = new DynamicRestClient(client);

            var result = await service.legislators.geo(lat: 44.926868, _long: -93.214049);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);

The end result is intended to be a coventions based way to interact with REST services, quickly and in a natural manner. It is currently rudimentarh and only supports the GET verb.
