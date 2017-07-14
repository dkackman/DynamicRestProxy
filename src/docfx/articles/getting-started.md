# DynamicRestProxy - Getting Started

This article describes the basic conventions of the DynamicRextProxy api.
See [advanced topics](advanced.md) for mechanisms to bypass these conventions.

## Basic Usage

Using the dynamic rest client start with instantiating an instance, accessing an endpoint path and invoking a REST verb, awaiting the result. Always declare the `DynamicRestClient` as
a `dynamic`.

    dynamic client = new DynamicRestClient("http://dev.virtualearth.net/REST/v1/");
    dynamic result = await proxy.Locations.get(postalCode: "55116", countryRegion: "US", key: "api-key");
[Figure 1]

    GET http://dev.virtualearth.net/REST/v1/Locations?postalCode=55116&countryRegion=US&key=api-key HTTP/1.1
    Accept: application/json, text/json, text/x-json, text/javascript
    Host: dev.virtualearth.net
    Accept-Encoding: gzip, deflate
    Connection: Keep-Alive

### Building the endpoint path

The endpoint path is represented by dot-separated members of the dynamic client instance. Each node in the path is another dynamic object
to which additional path elements can be chained. The resulting path is relative to the base address set in the constructor of the client object.

The full endpoint Uri of the example in Figure 1 is:

http://dev.virtualearth.net/REST/v1/Locations/

    dynamic google = new DynamicRestClient("https://www.googleapis.com/");
    dynamic bucket = await google.storage.v1.b("uspto-pair").get();
[Figure 2]

The code in Figure 2 chains multiple elements together to build a longer path. It also uses an escape mechanism in order to specify a
path element that is not a valid idenifier in C#. The resulting Uri is:

https://www.googleapis.com/storage/v1/b/uspto-pair/

### Passing parameters

Parameters are based to the verb method invocation using [C#'s named parameter syntax](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments#named-arguments).

Any type of object can be passed as a parameter value which are serialized via the value object's
[ToString](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring?view=netframework-4.7)
method. Both parameter names and values are [Url encoded](https://docs.microsoft.com/en-us/dotnet/api/system.net.webutility.urlencode?view=netframework-4.7)

The GET request for the example in Figure 1 is:
http://dev.virtualearth.net/REST/v1/Locations/?postalCode=55116&countryRegion=US&key=api-key

Named parameters passed to a POST invocation will be [form url encoded](http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.1) in the request body.

### Passing content

Request content is passed to the verb invocation as an unnamed argument. The first unnamed argument will be passed as the request
content body. Subsequent unnamed arguments, [with the exception of some special types](advanced.md), will be ignored.

- Strings and primitive types will be passed as [StringContent](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.stringcontent.-ctor?view=netframework-4.7)
- Byte arrays will be passed as [ByteArrayContent](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.bytearraycontent?view=netframework-4.7)
- Any type of [stream](http://msdn.microsoft.com/query/dev15.query?appId=Dev15IDEF1&l=EN-US&k=k(System.IO.Stream);k(DevLang-csharp)&rd=true) will be passed as [StreamContent](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.streamcontent.-ctor?view=netframework-4.7)
- A `IEnumerable<object>` will be sent as multi-part content, with each constituent object being serialized by the above rules
- All other types will be serialized to JSON

`    dynamic google = new DynamicRestClient("https://www.googleapis.com/calendar/v3/");
                
    dynamic calendar = new ExpandoObject();
    calendar.summary = "unit_testing";

    var result = await google.calendars.post(calendar);
[Figure 3]

The resulting request shows the serialized content:

    POST https://www.googleapis.com/calendar/v3/calendars HTTP/1.1
    Authorization: OAuth xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    Accept: application/json, text/json, text/x-json, text/javascript
    Content-Type: application/json; charset=utf-8
    Host: www.googleapis.com
    Expect: 100-continue
    Accept-Encoding: gzip, deflate
    Connection: Keep-Alive
    Content-Length: 26

    {"summary":"unit_testing"}

### Invoking the Http verb

GET, PUT, POST, DELETE and PATCH are the http verbs supported by this REST client. Invocation of the verb method
sends the appropraite http message to the endpoint, along with defaults, parameters and content. Verb methods are always
lower case and return a `Task` object, so must be `await`-ed. Unless using a strongly typed response
(see below), the return will be `Task<object>` where the result type is a dynamic object.

## Setting Defaults

Setting defaults to be included in every request is accomplished by passing a 
[DynamicRestClientDefaults](xref:DynamicRestProxy.PortableHttpClient.DynamicRestClientDefaults) to the client constructor.

### Api keys

Many REST apis require an api-key to use, typically as a paramter included on all requests. Setting it as a default parameter
will ensure it is added to every request.

    var defaults = new DynamicRestClientDefaults();
    defaults.DefaultParameters.Add("format", "json");
    defaults.DefaultParameters.Add("api_key", "my-api-key");
    defaults.DefaultParameters.Add("nojsoncallback", "1");

    dynamic flickr = new DynamicRestClient("https://api.flickr.com/services/rest/");
    dynamic user = await flickr.get(method: "flickr.people.findByUsername", username: "dkackman");

### Authentication and authorization

For services that require authentication and authorization, the defaults type can also be used to 
manage passing auth data.

    var defaults = new DynamicRestClientDefaults()
    {
        AuthScheme = "OAuth",
        AuthToken = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
    };

    dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults);
    dynamic profile = await google.oauth2.v1.userinfo.get();

The auth data is added to every request.    

    GET https://www.googleapis.com/oauth2/v1/userinfo HTTP/1.1
    Authorization: OAuth xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    Accept: application/json, text/json, text/x-json, text/javascript
    Host: www.googleapis.com
    Accept-Encoding: gzip, deflate

## Http Errors

In the event that the response message's [IsSuccessStatusCode](xref:System.Net.Http.HttpResponseMessage) property
is false the invocation method will throw a 
[DynamicRestClientResponseException](xref:DynamicRestProxy.PortableHttpClient.DynamicRestClientResponseException).
This exception includes the response method so additional details of the falure can be inspected.