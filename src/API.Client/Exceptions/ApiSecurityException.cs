using System;

namespace API.Client.Exceptions
{
    public class ApiSecurityException : Exception
    {
        public ApiSecurityException(string message) : base(message)
        {

        }
    }
}
