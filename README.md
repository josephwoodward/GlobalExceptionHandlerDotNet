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

## Setup

Within your `Startup.cs` file's `Configure` method (be sure to call before `UseMvc()`):

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app.UseWebApiGlobalExceptionHandler(x =>
        {
            x.ForException<PageNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound);
        });

        app.UseMvc();
    }
}
```

Returns the following default exception message:

```json
{
    "error": {
        "exception": "PageNotFoundException",
        "message": "Page could not be found"
    }
}
```

This exception message can be overridden via the `ExceptionFormatter` method like so:

```csharp

app.UseWebApiGlobalExceptionHandler(x =>
{
    x.ForException<PageNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound);
    x.MessageFormatter(exception => JsonConvert.SerializeObject(new
    {
        error = new
        {
            exception = exception.GetType().Name,
            message = exception.Message
        }
    }));
});
```

Alternatively you can set the formatter to be unique per exception registered. This will override the root `x.MessageFormatter` referenced above.

```csharp
app.UseWebApiGlobalExceptionHandler(x =>
{
    x.ForException<ArgumentException>().ReturnStatusCode(HttpStatusCode.BadRequest).UsingMessageFormatter(
        exception => JsonConvert.SerializeObject(new
        {
            error = new
            {
                message = "Oops, something went wrong"
            }
        }));
    x.MessageFormatter(exception => "This formatter will be overridden when an ArgumentException is thrown");
});
```

## Configuration Options:

- `OnError(Func<Exception, HttpContext, Task>)`  
Logging endpoint allowing you to capture exception information.
```csharp
// Example
x.OnError((exception, httpContext) =>
{
    _logger.Error(exception.Message);
    return Task.CompletedTask;
});
```

- `ContentType`  
Specify the returned content type (default is `application/json)`.

- `MessageFormatter(Func<Exception, string>)`  
Set a default message formatter, that any unhandled exceptions will trigger.

```csharp
x.MessageFormatter((exception) => {
    return "Oops, something went wrong! Check the logs for more information.";
});
```

## Content Negotiation

Because GlobalExceptionHandlerDotNet plugs into the .NET Core pipeline, it can also take advantage of content negotiation. This means that if a user requests a resource and sets the `Accept` header to `text/xml`, if an exception occurs then the content type will be formatted to the requested format type.

To enable content negotiation against ASP.NET Core MVC you will need to include [GlobalExceptionHandler.ContentNegotiation.Mvc](#)
