---
title: "Dont return api exceptions as bad requests"
categories:
  - Blog
tags:
  - .NET Core
  - .NET
  - Web Api
  - Api
---

So recently, when looking at some new projects that have web api or azure function http endpoints, I have noticed a lack of basic understanding of http knowledge that most api developers should have. This is the kind of details that can cause some major debugging and production level issues down the track. One of the subtle but important points is how exceptions are returned to an api client.

For this post, I will use .NET as an example.


**AVOID: Sending Bad Request (400) Status codes to a client as a catch-all**

Let's look at a webapi action that does some stuff, and catches exceptions, and returns any exception to the client.

```cs
[HttpPost("/route/blah/blah")]
public IActionResult SomeAwesomeApi()
{
     try {
       // do some stuff
       ...
     } catch(Exception ex){
        Return new BadRequestObjectResult(ex.Message);    // returns a 400 bad request regardless of server/client issue
     }
}
```

Now, there are a couple of issues with this:

1. `[.NET Specific]` The entire try/catch block is unneccassary, as a .NET Core middleware could be written to handle the exception across all actions, rather than repeating it.
2. `Red Flag`: Returning a `BadRequest` to the client for exceptions, which is a `HTTP 400` status code.

With (1) that's just a good practice for any platform to reduce code duplication. 

(2) is the main issue. The reason being is that an `HTTP 400` status means that the client request is invalid in some way. [see more about about HTTP 400](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400). This means that the client should modify it's request before repeating it.  Now in the example above where the `BadRequest` is returned on *any* exception, the exception could be caused from anything, server or client request issue. We don't know if it's due to the request, or due to something in the server code path. Perhaps network connectivity is down, there could just be something wrong with the server and nothing to do with the request. In which case we should never just send a 400 badrequest to the client. a *400 effectively means a client should not retry without changing the request*

> The only time we should send a BadRequest (HTTP 400) status to the client is when we know it's request related. For example properties don't validate, or a request header is missing or invalid.  An unknown exception should just return a 500 (Internal server Error)

Same example as an Azure Function HTTP Trigger Api Endpoint

```cs
[FunctionName("SomeFunction")]
[OpenApiOperation(operationId: "SomeFunction", tags: new[] { "name" })]
[OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Int32), Description = "an id parameter")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SomeResponse), Description = "Result")]
public async Task<IActionResult> SomeFunction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "route/blah/{id}")] HttpRequest req, ILogger log, int id)
{
    try
    {
        // do some stuff
        ...
    }
    catch(Exception ex)
    {
        return new BadRequestObjectResult(ex.Message);
    }

}
```
