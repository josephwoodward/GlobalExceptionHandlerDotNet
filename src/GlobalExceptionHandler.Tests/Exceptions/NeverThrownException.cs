using System;

namespace GlobalExceptionHandler.Tests.Exceptions
{
    public class NeverThrownException : Exception
    {
        public NeverThrownException()
        {
        }

        public NeverThrownException(string message) : base(message)
        {
        }

        public NeverThrownException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}