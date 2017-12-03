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

Version 2 now hangs off of the ASP.NET Core `UseExceptionHandler()` endpoint, adding a convention based API around it via the `WithConventions()` call:

```csharp
// Startup.cs

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseExceptionHandler().WithConventions(x => {
        x.ContentType = "application/json";
        x.MessageFormatter(s => JsonConvert.SerializeObject(new
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
app.UseExceptionHandler().WithConventions(x => {
    x.ContentType = "application/json";
    x.MessageFormatter(s => JsonConvert.SerializeObject(new
    {
        Message = "An error occurred whilst processing your request"
    }));

    x.ForException<RecordNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound);
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

### Per exception responses  

Or provide a custom error response for the exception type thrown:

```csharp
app.UseExceptionHandler().WithConventions(x => {
    x.ContentType = "application/json";
    x.MessageFormatter(s => JsonSerializer(new
    {
        Message = "An error occurred whilst processing your request"
    }));

    x.ForException<RecordNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound)
        .UsingMessageFormatter((ex, context) => JsonSerializer(new {
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
app.UseExceptionHandler().WithConventions(x => {
    x.ContentType = "application/json";
    x.MessageFormatter(s => JsonSerializer(new
    {
        Message = "An error occurred whilst processing your request"
    }));

    x.ForException<RecordNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound)
        .UsingMessageFormatter((ex, context) => JsonSerializer(new {
            Message = ex.Message
        }));
});
```

## Content Negotiation

GlobalExceptionHandlerDotNet plugs into the .NET Core pipeline, meaning you can also take advantage of content negotiation provided by the ASP.NET Core MVC framework, enabling the clients to dictate the preferred content type.

To enable content negotiation against ASP.NET Core MVC you will need to include the [GlobalExceptionHandler.ContentNegotiation.Mvc](https://www.nuget.org/packages/GlobalExceptionHandler.ContentNegotiation.Mvc/) package.

Note: Content negotiation is handled by ASP.NET Core MVC so this takes a dependency on MVC.

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

- `MessageFormatter(...)`  
Set a default message formatter that any unhandled exception will trigger.

```csharp
x.MessageFormatter((ex, context) => {
    return "Oops, something went wrong! Check the logs for more information.";
});
```

- `DebugMode`
Enabling debug mode will cause GlobalExceptionHandlerDotNet to return the full exception thrown. **This is disabled by default and should not be set in production.**
