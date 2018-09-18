# Global Exception Handling for ASP.NET Core

[![Build status](https://ci.appveyor.com/api/projects/status/kdbepiak0m6olxw7?svg=true)](https://ci.appveyor.com/project/JoeMighty/globalexceptionhandlerdotnet)

GlobalExceptionHandlerDotNet allows you to configure exception handling as a convention with your ASP.NET Core application pipeline as opposed to explicitly handling them within each controller action. This could be particularly helpful in the following circumstances:

- Reduce boiler plate try-catch logic in your controllers
- Catch and appropriately handle exceptions outside of the ASP.NET Core framework
- You don't want error codes being visible by consuming APIs (return 500 for every exception)

This middleware targets the ASP.NET Core pipeline with an optional dependency on the MVC framework for content negotiation if so desired.

## Installation

GlobalExceptionHandler is [available on NuGet](https://www.nuget.org/packages/GlobalExceptionHandler/) and can be installed via the below commands depending on your platform:

```
$ Install-Package GlobalExceptionHandler
```
or via the .NET Core CLI:

```
$ dotnet add package GlobalExceptionHandler
```

## Bare Bones Setup

```csharp
// Startup.cs

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseGlobalExceptionHandler(x => {
        x.ContentType = "application/json";
        x.ResponseBody(s => JsonConvert.SerializeObject(new
        {
            Message = "An error occurred whilst processing your request"
        }));
    });
    
    app.Map("/error", x => x.Run(y => throw new Exception()));
}
```

Any exception thrown by your application will result in the follow response:

```http
HTTP/1.1 500 Internal Server Error
Date: Fri, 24 Nov 2017 09:17:05 GMT
Content-Type: application/json
Server: Kestrel
Cache-Control: no-cache
Pragma: no-cache
Transfer-Encoding: chunked
Expires: -1

{
  "Message": "An error occurred whilst processing your request"
}
```

## Handling specific exceptions

You can explicitly handle exceptions like so:

```csharp
app.UseGlobalExceptionHandler(x => {
    x.ContentType = "application/json";
    x.ResponseBody(s => JsonConvert.SerializeObject(new
    {
        Message = "An error occurred whilst processing your request"
    }));

    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound);
});
```

```http
HTTP/1.1 404 Not Found
Date: Sat, 25 Nov 2017 01:47:51 GMT
Content-Type: application/json
Server: Kestrel
Cache-Control: no-cache
Pragma: no-cache
Transfer-Encoding: chunked
Expires: -1

{
  "Message": "An error occurred whilst processing your request"
}
```

#### Runtime Status Code

If talking to a remote service, you could optionally choose to forward the status code on, or propagate it via the exception using the following `ToStatusCode(..)` overload:

```csharp
app.UseGlobalExceptionHandler(x =>
{
    x.ContentType = "application/json";
    x.Map<HttpServiceException>().ToStatusCode(ex => ex.StatusCode).WithBody((e, c) => "Something went wrong");
    ...
});
```

### Per exception responses  

Or provide a custom error response for the exception type thrown:

```csharp
app.UseGlobalExceptionHandler(x => {
    x.ContentType = "application/json";
    x.ResponseBody(s => JsonConvert.SerializeObject(new
    {
        Message = "An error occurred whilst processing your request"
    }));

    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound)
        .WithBody((ex, context) => JsonConvert.SerializeObject(new {
            Message = "Record could not be found"
        }));
});
```

Response:

```json
HTTP/1.1 404 Not Found
...
{
  "Message": "Record could not be found"
}
```

Alternatively you could output the exception content if you prefer:

```csharp
app.UseGlobalExceptionHandler(x => {
    x.ContentType = "application/json";
    x.ResponseBody(s => JsonConvert.SerializeObject(new
    {
        Message = "An error occurred whilst processing your request"
    }));

    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound)
        .WithBody((ex, context) => JsonConvert.SerializeObject(new {
            Message = ex.Message
        }));
});
```

## Content Negotiation

GlobalExceptionHandlerDotNet plugs into the .NET Core pipeline, meaning you can also take advantage of content negotiation provided by the ASP.NET Core MVC framework, enabling the clients to dictate the preferred content type.

To enable content negotiation against ASP.NET Core MVC you will need to include the [GlobalExceptionHandler.ContentNegotiation.Mvc](https://www.nuget.org/packages/GlobalExceptionHandler.ContentNegotiation.Mvc/) package.

Note: Content negotiation is handled by ASP.NET Core MVC so this takes a dependency on MVC.

```csharp
//Startup.cs

public void ConfigureServices(IServiceCollection services)
{
    services.AddMvcCore().AddXmlSerializerFormatters();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseGlobalExceptionHandler(x =>
    {
        x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound)
            .WithBody(e => new ErrorResponse
            {
                Message = e.Message
            });
    });

    app.Map("/error", x => x.Run(y => throw new RecordNotFoundException("Record could not be found")));
}
```

Now when an exception is thrown and the consumer has provided the `Accept` header:

```http
GET /api/demo HTTP/1.1
Host: localhost:5000
Accept: text/xml
```

The response will be formatted according to the `Accept` header value:

```http
HTTP/1.1 404 Not Found
Date: Tue, 05 Dec 2017 08:49:07 GMT
Content-Type: text/xml; charset=utf-8
Server: Kestrel
Cache-Control: no-cache
Pragma: no-cache
Transfer-Encoding: chunked
Expires: -1

<ErrorResponse 
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
  xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Message>Record could not be found</Message>
</ErrorResponse>
```

## Logging

Under most circumstances you'll want to keep a log of any exceptions thrown in your log aggregator of choice. You can do this via the `OnError` endpoint:

```csharp
x.OnError((exception, httpContext) =>
{
    _logger.Error(exception.Message);
    return Task.CompletedTask;
});
```

## Configuration Options:

- `ContentType`  
Specify the returned content type (default is `application/json)`.

- `ResponseBody(...)`  
Set a default response body that any unhandled exception will trigger.

```csharp
x.ResponseBody((ex, context) => {
    return "Oops, something went wrong! Check the logs for more information.";
});
```

- `DebugMode`
Enabling debug mode will cause GlobalExceptionHandlerDotNet to return the full exception thrown. **This is disabled by default and should not be set in production.**
