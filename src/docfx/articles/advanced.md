# Advanced Topics

The thing about convetion based api's is that the conventions both hide the complexity but also the flexiblity of the
underlying mechanisms. Conventions favor simplicity over expressiveness; making the common things easy, but they must also 
make the less common possible.

## Escape Mechanisms

The following escape mechanisms exist to expose the full flexibility of http/Rest and also work around
places where a it may conflict with the rules and syntax of C#.

### Path names

There are times when a Uri path segment is not a valid C# identifier. In those cases, the path segment can be specified as a string
chained to rest of the path specification as a string. String escaped segments can appear anywhere in the path chain.

    google.storage.v1.b("uspto-pair").get();
    google.storage.("v1").b("uspto-pair").get();

This is also useful when part of the path is a data point not known at compile time.

    string name = "John Smith";
    dynamic example = new DynamicRestClient("https://www.example.com/");
    dynamic person = example.restapi.people(name).get();

### Reserved word parameter names

In cases where a parameter name is a C# reserved word it can be escaped using `@`.

    dynamic openstates = new DynamicRestClient("http://openstates.org/api/v1/");
    var result = await openstates.legislators.geo.get(lat: 44.926868, @long: -93.214049);

### Illegal identifier parameter names

Rest parameters can also contain characters that make them illegal identifiers altogether in C#. In this case named parameter syntax won't work but paramters can be passed as a Dictionary.

    dynamic sunlight = new DynamicRestClient("http://congress.api.sunlightfoundation.com");
    var parameters = new Dictionary<string, object>()
    {
        { "chamber", "senate" },
        { "history.house_passage_result", "pass" }
    };

    dynamic result = await sunlight.bills.get(paramList: parameters);

Since only named arguments are parsed as parameters, the dictionary must be passed as a named argument event though the name is irrelevent.
Named and dictionary based parameters can be mixed in the same invocation.

### The HttpMessageHandler and HttpClient instances

By default the dynamic client will use the [HttpClientHandler](xref:System.Net.Http.HttpClientHandler)
when creating the internal [HttpClient](xref:System.Net.Http.HttpClient). If you need to use a different
[HttpMessageHandler](xref:System.Net.Http.HttpMessageHandler) derived type, an instance can be passed to the dynamic client construcutor.

If fine grained configuraiton of the `HttpClient` is needed, there is also a constructor overload that accepts an 'HttpClient' instance.

The unit tests use this extensively in order to use fake http responses rather than tightly coupling the tests to the endpoints.

    public async Task CoordinateFromPostalCode()
    {
        using (var client = new HttpClient(MockInitialization.Handler))
        {
            client.BaseAddress = new Uri("http://dev.virtualearth.net/REST/v1/");

            string key = CredentialStore.RetrieveObject("bing.key.json").Key;

            dynamic virtualearth = new DynamicRestClient(client);
            var result = await virtualearth.Locations.get(postalCode: "55116", countryRegion: "US", key: key);

            Assert.IsTrue(result.resourceSets.Count > 0);
            Assert.IsTrue(result.resourceSets[0].resources.Count > 0);

            var r = result.resourceSets[0].resources[0].point.coordinates;
            Assert.IsTrue((44.9108238220215).AboutEqual((double)r[0]));
            Assert.IsTrue((-93.1702041625977).AboutEqual((double)r[1]));
        }
    }

### Request configuration callback

For fine grained control of the `HttpRequestMessage` a callback can be provided in the constructor of the dynamic client.
This function will be called just prior to every REST invocation.

The callback must have the signature of `Func<HttpRequestMessage, CancellationToken, Task>`. It will be `await`ed by the dyanmic client
before the request message is sent.

    dynamic client = new DynamicRestClient("http://dev.virtualearth.net/REST/v1/",
        configureRequest: (request, token) =>
        {
            Debug.WriteLine(request.RequestUri);
            return Task.CompletedTask;
        });

    dynamic result = await client.Locations.get(postalCode: "55116", countryRegion: "US", key: "key");

Other uses of the callback are checking or refreshing auth tokens etc, signaling the UI that communication is happening, logging and debugging.

### Calling a POST endpoint with url parameters

By default, Uri parameters on a POST request are url form encoded and sent in the request body. There are cases where a POST request has other data in the request body but also has parameters in the Uri. In this case the parameter can be based in a [PostUrlParam](xref:DynamicRestProxy.PortableHttpClient.PostUrlParam).

## Setting Content Headers

It is common that the MIME type or other headers needs to be set when uploading content to a REST endpoint.
To do so, content can be passed as [ContentInfo](xref:DynamicRestProxy.PortableHttpClient.ContentInfo) or [SteamInfo](xref:DynamicRestProxy.PortableHttpClient.StreamInfo) objects. 
These types allow MIME type and other headers to be specified with the content.

    using (var stream = new StreamInfo(File.OpenRead(@"D:\temp\test.png"), "image/png"))
    {
        dynamic google = new DynamicRestClient("https://www.googleapis.com/");
        dynamic result = await google.upload.storage.v1.b.unit_tests.o.post(stream, name: new PostUrlParam("test_object"), uploadType: new PostUrlParam("media"));
    }

## Bypassing Content Conventions

If you require fine grained control over the request content, any instance of an
[HttpContent](https://msdn.microsoft.com/en-us/library/system.type(v=vs.110).aspx) derived
class passed to the verb invocation will be added to the reuqest as-is, overriding any content creation conventions.

If, for instance, an endpoint does not accept Json, an object could be serialized as Xml
and POSTed using this mechansim.

## Returning Types Other Than Dynamic

Pass a single [Type](https://msdn.microsoft.com/en-us/library/system.type(v=vs.110).aspx) instance to a verb invocation in order
to control how response conent is deserialized or overriden.

### Strongly typed response

If you have a POCO type that you would like the response deserialied to, pass the type of that class to the rest invocation.
The response body, assuming it is Json, will be deserialized into that type using [JSON.NET](https://www.newtonsoft.com/json).

    public class Bucket
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string selfLink { get; set; }
        public string name { get; set; }
        public DateTime timeCreated { get; set; }
        public int metageneration { get; set; }
        public string location { get; set; }
        public string storageClass { get; set; }
        public string etag { get; set; }
    }

    dynamic google = new DynamicRestClient("https://www.googleapis.com/");
    Bucket bucket = await google.storage.v1.b("uspto-pair").get(typeof(Bucket));
    Console.WriteLine(bucket.location);

### Specifying other return types

- Passing `typeof` `Stream`, `byte[]` or `string` will deserialize the request content to those respective types
- Passing `typeof` [HttpResponseMessage](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage?view=netframework-4.7) allows you to receive the complete response message
- Any other [Type](xref:System.Type) instance is assumed to be intended for a strongly typed response as discussed above

## Special Unamed Argument Types

The following types, when passed as unnamed arguments will be used during the creation and invocation of the request.

- `JsonSerializerSettings` Serialization is done with [Json.Net](http://www.newtonsoft.com/json). An instance of this type can be passed to control serialization
- [CancellationToken](http://msdn.microsoft.com/query/dev15.query?appId=Dev15IDEF1&l=EN-US&k=k(System.Threading.CancellationToken);k(SolutionItemsProject);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.6.1);k(DevLang-csharp)&rd=true) Because verb invocations are always `async`, a `CancellationToken` can be passed in order for client code to cancel the operation