using System;
namespace Common.Utilities.Exceptions
{
    [Serializable]
    public class UnverifiedException : Exception
    {
        public UnverifiedException() { }

        public UnverifiedException(string message) : base(message) { }

        public UnverifiedException(string message, Exception inner) : base(message, inner) { }
    }
}
