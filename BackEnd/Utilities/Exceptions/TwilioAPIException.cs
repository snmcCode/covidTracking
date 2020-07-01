using System;
namespace BackEnd.Utilities.Exceptions
{
    [Serializable]
    public class TwilioAPIException : Exception
    {
        public TwilioAPIException() { }

        public TwilioAPIException(string message) : base(message) { }

        public TwilioAPIException(string message, Exception inner) : base(message, inner) { }
    }
}
