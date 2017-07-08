# DynamicRestProxy - Getting Started
This article describes the basic conventions of the DynamicRextProxy api.
See [advanced topics](advanced.md) for mechansims to bypass these conventions.

## Basic Usage

Using the dynamic rest client start with instantiating an instance, accessing an endpoint path and invoking a REST verb, awaiting the result. Always declare the DynamicRestClient as 
a dynamic.

    dynamic client = new DynamicRestClient(http://dev.virtualearth.net/REST/v1/");
    var result = await proxy.Locations.get(postalCode: "55116", countryRegion: "US", key: "api-key");
Figure 1

### Building the endpoint path

The endpoint path is represented by dot seperated members of the dynamic client instance. Each node in the path is another dynamic object
to which additional path elements can be chained. The resulting path is relative to the base address set in the constructor of the client object.

The full endpoint Uri of the example in figure 1 is:
http://dev.virtualearth.net/REST/v1/Locations/

### Passing parameters

Parameters are based to the verb invocation using [C#'s named paramter syntax](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments#named-arguments).

Parameter names are passed as is. Any type of object can be passed as a parameter value. Values are serialized as whatever the type's 
[ToString](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring?view=netframework-4.7)
method returns. Both parameter names and values are [Url encoded](https://docs.microsoft.com/en-us/dotnet/api/system.net.webutility.urlencode?view=netframework-4.7).

The GET request for the example in figure 1 is:
http://dev.virtualearth.net/REST/v1/Locations/?postalCode=55116&countryRegion=US&key=api-key

Named parameters passed to a POST invocation will be [form url encoded](http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.1) in the request body.

### Passing Content

Request content is passed to the verb invocation as an unnamed argument. The first unnamed argument will be passed as the request
content body. Subsequent unnamed arguments, [with the exception of some special types](advanced.md), will be ignored.

Strings and primitive types will be passed as [StringContent](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.stringcontent.-ctor?view=netframework-4.7). 
Byte arrays will be passed as [ByteArrayContent](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.bytearraycontent?view=netframework-4.7)
Any type of [stream](http://msdn.microsoft.com/query/dev15.query?appId=Dev15IDEF1&l=EN-US&k=k(System.IO.Stream);k(DevLang-csharp)&rd=true) will be passed as [StreamContent](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.streamcontent.-ctor?view=netframework-4.7).
All other types will be serialized to JSON.

#### Setting content headers

### Invoking the Http verb

GET, PUT, POST, DELETE and PATCH are the http verbs supported by this REST client. Invocation of the verb method
sends the appropraite http message to the endpoint, along with defaults, parameters and content. Verb methods are always 
lower case and return a <code>Task</code> object, so must be <code>await</code>-ed. Unless using a strongly typed response
(see below), the return will be <code>Task&lt;object></code> where the result type is a dynamic object.

## Setting Defaults

### Api Keys

### Authentication and Authorization

