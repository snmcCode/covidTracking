using System;
namespace Common.Utilities.Exceptions
{
    [Serializable]
    public class SqlDatabaseDataNotFoundException : Exception
    {
        public SqlDatabaseDataNotFoundException() { }

        public SqlDatabaseDataNotFoundException(string message) : base(message) { }

        public SqlDatabaseDataNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
