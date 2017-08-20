# Global Exception Handling for ASP.NET Core

GlobalExceptionHandlerDotNet allows you to configure exceptions handling as a convention as opposed to explicitly within each controller action. This could be particularly helpful in the following circumstances:

- You're using a command-handler pattern such as MediatR
- You don't want error codes being visible by consuming APIs (return 500 for every exception)
- Reduce boiler plate try-catch logic in your controllers

This middleware supports both **WebAPI** and **MVC** type projects.

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

This exception message can be overridden via the `ExceptionFormatter` method. 

**Configuration Options:**

- `ContentType` - Specify the returned content type (default is `application/json)`.

- `MessageFormatter(Func<Exception, string>)` - Overrides default JSON message formatter.

```csharp
x.MessageFormatter((exception) => {
    return "Oops, something went wrong!";
});
```

## MVC Setup

Work in progress.