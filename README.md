Dynamic Rest
================
[![NuGet](https://img.shields.io/nuget/dt/DynamicRestClient)](https://www.nuget.org/packages/DynamicRestClient/)

- [Documentation](https://dkackman.github.io/DynamicRestProxy)

A conventions based rest client using the .NET [Dynamic Language Runtime](http://msdn.microsoft.com/en-us/library/dd233052(v=vs.110).aspx). 

The [documentation](https://dkackman.github.io/DynamicRestProxy/) has further detail and examples, as does [this article](http://www.codeproject.com/Articles/762189/A-Dynamic-Rest-Client-Proxy-with-the-DLR) on codeproject.

This is a set of classes that wrap a concrete implementation of http client communication with a [DynamicObject](http://msdn.microsoft.com/en-us/library/system.dynamic.dynamicobject(v=vs.110).aspx). The wrapper translates dynamic method invocations and endpoint paths into REST requests. 

All requests are asynynchronous and return Task objects.

The intent is to make it easier to access REST API's from C# without needing to create strongly typed API wrappers and numerous static POCO types for basic DTO responses. 

So a GET statement can be as simple as:

    dynamic google = new DynamicRestClient("https://www.googleapis.com/");
    dynamic bucket = await google.storage.v1.b("uspto-pair").get();
    Console.WriteLine(bucket.location);

Or if you insist on static DTO types, a type argument can be supplied (deserialization uses [Json.Net](http://json.codeplex.com/) so all its rules and patterns apply):

    dynamic google = new DynamicRestClient("https://www.googleapis.com/");
    Bucket bucket = await google.storage.v1.b("uspto-pair").get(typeof(Bucket));
    Console.WriteLine(bucket.location);

Supports the GET, POST, PUT, PATCH and DELETE verbs.

Tested on dotnetcore on Linux.

If you try to run the unit tests take a close look at the CredentialStore class in the unit test project. It's pretty straighforward and you can use it to supply your own api keys while keeping them out of the code.

