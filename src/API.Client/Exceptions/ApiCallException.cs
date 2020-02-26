using System;

namespace API.Client.Exceptions
{
    public class ApiCallException : Exception
    {
        public ApiCallException(string message) : base(message)
        {

        }

        public ApiCallException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
