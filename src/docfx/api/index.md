# DynamicRestProxy API

Interacting with the DynamicRestProxy api is primarly via dynamic nature of the
[DynamicRestClient](xref:DynamicRestProxy.PortableHttpClient.DynamicRestClient) type. In most cases
declare the client instance as type [dynamic](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic)
at which point all property and method invocations are dyanmically invoked.

    dynamic client = new DynamicRestClient("https://example.com");