# DynamicRestProxy API

Interaction with the DynamicRestProxy api is primarly via the dynamic nature of the
[DynamicRestClient](xref:DynamicRestProxy.PortableHttpClient.DynamicRestClient) type. In most cases
declare the client instance as type [dynamic](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic)
at which point all property and method invocations are dynamically invoked.

    dynamic client = new DynamicRestClient("https://example.com");
    dynamic result = await client.get();

## Helper Types

- [ContentInfo](xref:DynamicRestProxy.PortableHttpClient.ContentInfo) Used to declare the MIME type and content heanders of message content
- [DynamicRestClientDefaults](xref:DynamicRestProxy.PortableHttpClient.DynamicRestClientDefaults) Default values that will be added to all requests
- [DynamicRestClientResponseException](xref:DynamicRestProxy.PortableHttpClient.DynamicRestClientResponseException) Exception thrown when response status does not indicate success. Allows response content and headers to be inspected on failure
- [HttpClientExtensions](xref:DynamicRestProxy.PortableHttpClient.HttpClientExtensions) Extension methods to aid deserialization
- [PostUrlParam](xref:DynamicRestProxy.PortableHttpClient.PostUrlParam) By default POST parameters will be form encoded. Use this to force the request to have a particular parameter encoded on the url query
- [StreamInfo](xref:DynamicRestProxy.PortableHttpClient.StreamInfo) Wrapper class for a Stream that relates the stream to metadata (MIME type) about the stream so metadata can be added to content headers