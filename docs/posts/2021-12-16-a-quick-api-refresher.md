---
title: "A Quick Web API REST Refresher"
categories:
  - Blog
tags:
  - .NET Core
  - .NET
  - Web Api
  - Api
  - HTTP
  - Endpoints
  - REST
  - RPC
  - nodejs
---

So, I have been creating and working with web api's since the early 2000's. First with perl cgi-bin requests, PHP simple RPC style POST/GET requests, than SOAP with PHP and .NET, than .NET http handlers with xml, binary, csv, etc.....and over the last decade web apis with .NET MVC, WebApi's, NodeJs with JSON, and I have seen all arrays and different approaches to implementation.

Ever since we came into the Web Api era around 2009, there was a big shift to using the `REST` pattern and terminology with `json`, sure we have things like `Graphql` now, though for the most part `REST` is a fairly simple pattern to use. However it's often misunderstood even by experienced developers.

`REST` is really just designed to work the way the stateless `HTTP` protocol was designed for. Contrary to popular belief, It doesn't force certain approaches, rather just uses http the way it was intended. Using verbs, resources, http status codes and the like.

Though some popular patterns have emerged via `REST` that make it simple and consistent to apply, irrespective of platform used. Let's look at some now.

Rest deals with resources, now a resource could represent an entity record in a database, it could represent a process, a job, task, a collection of items. typically a resource refers to a single element. Though a resource, could have sub-resources and the like. 

### Let's look at `HTTP` verbs in the REST context:

- `POST` used for creating resources. 
- `GET` used to query resources, get a collection, get a single item (with an id), with filters and so on.
- `PUT` for updating a specific resource
- `DELETE` to delete/remove a specific resource
- `PATCH` for making amendments, changes to specific parts of a resource

### Now lets look at the most popular `HTTP Status Codes` :

Anything in `2xx` range is a Success status

- `200 OK` - everything's a-ok, request and process is success
- `201 Created` - ok, we have created a new resource that's ready, and a link to that resources metadata/status will be in the responses `location` header.
- `202 Accepted` - ok, we acknowledge the request though may not have fulfilled it yet, it could be a long running process, however we have a link in the `location` header to obtain completion status, and possible `retry` headers.  A client app can potentially poll this `location` to see when it's completed or reached another status ( note: this is http semantics, not talking about event messaging and the like at this point )
- `204 No Content` - ok success, though we have no content to return..

Now `4xx` codes represent an issue with the client request

- `400 Bad Request` - something in the clients request is malformed, invalid, or simply incorrect, often used for field validation. This means a client should not retry a request without changing something in it.
- `401 Unauthorized` - the client has not sent up adequate authorization, be it an `Authorization` header, or a cookie, or it's expired, etc. Usually an indication for a client to login.

And `5xx` indicate a server error of some sort. They may be transient errors, and a retry could potentially work.

- `500 Internal server error` - unhandled or unknown server errors, exceptions and the like. (dont use 400's for these).
- `502 Bad Gateway` - could be a range of issues, from the web server, to proxies, dns etc.


### Ok got it, so now look at how they typically work.

- `POST` is used to create a new resource with data in body, the server returns http `201 Created` for a resource that is created and ready, or `202 Accepted` to indicate acknoledgement of the request, though resource may be long running. In both instances the url to retrieve the resources metadata/status via a `GET` is supplied in the `location` header. 
- `GET` is used to query a collection of resource items, or a single item with an id... it typically returns a `200 OK`. Now when using `GET` with an explicit `id` to fetch a single item, this will often (not always) represent the same url route as what the `POST` returns in the `location` header.
- `PUT` is used to update an explicit resource. It could return a `200 OK` or `204 No Content`.
- `PATCH` is not used as often, however it is similar to a `PUT` except where a put is about updating a resource, `PATCH` is designed for updating specific parts of a resource, lets say explicit fields, perhaps a status etc.
- `DELETE` is to delete/remove/cancel an explicit resource. It could return the deleted resource with a `200 OK` or just confirmation with a `204 No Content`.

> Notice the key point is when using `POST`, the webapi should provide the `location` header with the url to the resource created/acknoledged

### Ahhh so what about REST routes

Now routing is where things get interesting... often devs will call their API's `REST` based because they use verbs and response codes, however their routing conventions may be more aligned with `RPC` (Remote Procedure Call) approaches.

As mentioned before `REST` does not enforce particular conventions, however recommends certain concepts. Let's look at a typical REST convention example.

Let's say we have a resource called products, in rest we typically use the pluralized form **"products"** to represent a collection of product in a similar way to how database tables are often named (NOTE: remember REST uses resources, does not neccassarily have to tie back to database records.). This way we can apply consistent routing conventions regardless of the resource for most CRUD like operations.

> NOTE: before you consider that CRUD won't suit your particular services, and therefore REST wont work. CRUD here is just used as an example for a base set of operations, as it's familiar for database operations.

**Example REST-like routing style:**

```
POST /api/products
GET /api/products
GET /api/products/?optionalFilter1=filterValue&limit=10
GET /api/products/{id}
PUT /api/products/{id}
PATCH /api/products/{id}
DELETE /api/products/{id}

```

With this approach in this example there are effectively 2 routes that use different HTTP VERBS to determine the actual operation on the resource. One route defines the resource collection, the other defines an explicit item within that resource collection

```
/api/products       - resource collection
/api/products/{id}  - explicit item within resource collection
```

> REST style API routes define resources, and HTTP Verbs define operations. REST came about as a way to provide consistency and standardization for a web based http environment.


**Now contrast this with an `RPC` style api routing**

```
POST /api/addproduct
GET /api/getproductslist
GET /api/getproduct/{id}
POST /api/updateproduct
DELETE /api/deleteproduct
```

Now with the `RPC` routing, we have 5 routes, indicating the operation, and using potentially any verb, ie `POST` is often used instead of `PUT`, and even sometimes `DELETE`.... the core point being that the route itself defines the operation.... its akin to calling a function, or procedure by name as in `Remote Procedure Call`.   

Now `RPC` is essentially the style many traditional developers have used to call API's for decades as it translates to older traditional API's.

> RPC style API routes define operations, HTTP Verbs are typically POST/GET, though can be any combination.

Sometimes there is a mix of REST/RPC styles in routing.

### Recommendation:

Aim for consistent, and standardized web practices, in essence `REST` is very helpful in this regard, especially as API's grow in scale and functionality. The larger the API the more you need consistency.


### References:

- For more details on Http status codes, look at the Mozilla [HTTP status page](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status).
- http://apihandyman.io/do-you-really-know-why-you-prefer-rest-over-rpc/
- https://restcookbook.com/
- https://cloud.google.com/blog/products/application-development/rest-vs-rpc-what-problems-are-you-trying-to-solve-with-your-apis
- https://blog.jscrambler.com/rpc-style-vs-rest-web-apis/


### REST Api Examples:

- https://docs.github.com/en/rest
- https://developer.okta.com/docs/reference/core-okta-api/

