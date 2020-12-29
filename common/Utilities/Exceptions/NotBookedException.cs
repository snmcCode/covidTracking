using System;
namespace Common.Utilities.Exceptions
{
    [Serializable]
    public class NotBookedException : Exception
    {
        public NotBookedException() { }

        public NotBookedException(string message) : base(message) { }

        public NotBookedException(string message, Exception inner) : base(message, inner) { }
    }
}
