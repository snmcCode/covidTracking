using System;
namespace Common.Utilities.Exceptions
{
    [Serializable]
    public class SqlDatabaseException : Exception
    {
        public SqlDatabaseException() { }

        public SqlDatabaseException(string message) : base(message) { }

        public SqlDatabaseException(string message, Exception inner): base(message, inner) { }
    }
}
