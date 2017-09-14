# Global Exception Handling for ASP.NET Core

GlobalExceptionHandlerDotNet allows you to configure exceptions handling as a convention as opposed to explicitly within each controller action. This could be particularly helpful in the following circumstances:

- Reduce boiler plate try-catch logic in your controllers
- Catch and appropriately handle exceptions outside of the MVC/WebAPI framework
- You don't want error codes being visible by consuming APIs (return 500 for every exception)

This middleware currently supports **WebAPI** with **MVC** support in the works.

## Installation

GlobalExceptionHandler is [available on NuGet](https://www.nuget.org/packages/GlobalExceptionHandler/) and can be installed via the below commands depending on your platform:

```
$ Install-Package GlobalExceptionHandler
```
or via the .NET Core CLI:

```
$ dotnet add package GlobalExceptionHandler
```

## Web API Setup

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

Alternatively you can set the formatter to be unique per exception registered. This will overwrite the root `MessageFormatter` referenced above.

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
    x.MessageFormatter(exception => "This will now be overridden when a PageNotFoundException is thrown");
});
```

**Configuration Options:**

- `ContentType` - Specify the returned content type (default is `application/json)`.

- `MessageFormatter(Func<Exception, string>)` - Overrides default JSON message formatter; this is useful if you want to change the error response format or type (XML for instance).

```csharp
x.MessageFormatter((exception) => {
    return "Oops, something went wrong! Check the logs for more information.";
});
```

## MVC Setup

Work in progress.