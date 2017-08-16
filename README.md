# GlobalExceptionHandler for ASP.NET Core

GlobalExceptionHandler allows you to configure exceptions handling as a convention as opposed to explicitly within each controller action. This could be particularly helpful in the following circumstances:

- You're using a command-handler pattern such as MediatR
- You don't want error codes being visible by consuming APIs
- Reduce boiler plate try-catch login in your controllers