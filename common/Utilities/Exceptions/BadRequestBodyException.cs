using System;
namespace Common.Utilities.Exceptions
{
    [Serializable]
    public class BadRequestBodyException : Exception
    {
        public BadRequestBodyException() { }

        public BadRequestBodyException(string message) : base(message) { }

        public BadRequestBodyException(string message, Exception inner) : base(message, inner) { }
    }
}
