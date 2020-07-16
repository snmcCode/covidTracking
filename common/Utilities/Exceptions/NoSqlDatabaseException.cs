using System;
namespace Common.Utilities.Exceptions
{
    [Serializable]
    public class NoSqlDatabaseException : Exception
    {
        public NoSqlDatabaseException() { }

        public NoSqlDatabaseException(string message) : base(message) { }

        public NoSqlDatabaseException(string message, Exception inner) : base(message, inner) { }
    }
}
