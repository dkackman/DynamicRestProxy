# Advanced Topics
The thing about convetion based api's is that the conventions hide both the complexity but also flexiblity of the
underlying mechanisms. Conventions favor simplicity over expressiveness making the simple things easy, but they also need 
to make the less simple possible.

## Escape Mechanisms
The following escape mechanisms exist to expose the full flexibility of http/Rest and also work around
places where a it may conflict with the rules and syntax of C#.

### Path names

### Reserved word parameter names

### Request configuration callback

### Calling a POST endpoint with url parameters

## Bypassing content serialization conventions
If you require fine grained control over the reqeust content any instance of an
[HttpContent](https://msdn.microsoft.com/en-us/library/system.type(v=vs.110).aspx) derived
class passed to the verb invocation will be added to the reuqest as-is, overriding any content creation conventions.

## Returning types other than dynamic 
Pass a single [Type](https://msdn.microsoft.com/en-us/library/system.type(v=vs.110).aspx) instance to a verb invocation in order
to control how response conent is deserialized or overriden.

### Strong typing the response

### Other return types
- Passing typeof Stream, byte[] or string will deserialize the request content to those respective types
- Passing typeof [HttpResponseMessage](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage?view=netframework-4.7) allows you to receive the complete response message
- Any other type instance is assumed to be intended for a strongly typed response as discussed above

## Special invocation argument types
The following types, when passed as unnamed arguments will be used during the creation and invocation of the request.
- JsonSerializerSettings Serialization is done with [Json.Net](http://www.newtonsoft.com/json). An instance of this type can be passed to control serialization.
- [CancellationToken](http://msdn.microsoft.com/query/dev15.query?appId=Dev15IDEF1&l=EN-US&k=k(System.Threading.CancellationToken);k(SolutionItemsProject);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.6.1);k(DevLang-csharp)&rd=true)
Because verb invocations are always async, a CancellationToken can be passed in order for client code to cancel the operation.
