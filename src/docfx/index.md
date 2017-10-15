# Dynamic Rest

A conventions based rest client using the .NET
[Dynamic Language Runtime](http://msdn.microsoft.com/en-us/library/dd233052(v=vs.110).aspx).

This is a set of classes that wrap a concrete implementation of http client communication with a
[DynamicObject](http://msdn.microsoft.com/en-us/library/system.dynamic.dynamicobject(v=vs.110).aspx).
The wrapper translates dynamic method invocations and endpoint paths into REST requests.

All requests are asynynchronous and return [Task](https://msdn.microsoft.com/en-us/library/system.threading.tasks.task(v=vs.110).aspx) objects.

The intent is to make it easier to access REST API's from C# without needing to create strongly typed API wrappers and
numerous static POCO types just to transfer basic DTO responses.

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

## Quick Start Notes

1. [Getting Started](articles/getting-started.md)
1. [Advanced Topics](articles/advanced.md)
1. [Api Documentation](api/index.md)
1. [NuGet Package](https://www.nuget.org/packages/DynamicRestProxy/)
1. [GitHub Project](https://github.com/dkackman/DynamicRestProxy/)
1. [Some Background](https://www.codeproject.com/Articles/762189/A-Dynamic-Rest-Client-Proxy-with-the-DLR)