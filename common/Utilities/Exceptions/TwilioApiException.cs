using System;
namespace Common.Utilities.Exceptions
{
    [Serializable]
    public class TwilioApiException : Exception
    {
        public TwilioApiException() { }

        public TwilioApiException(string message) : base(message) { }

        public TwilioApiException(string message, Exception inner) : base(message, inner) { }
    }
}
