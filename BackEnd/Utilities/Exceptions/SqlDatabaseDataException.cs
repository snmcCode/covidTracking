using System;
namespace BackEnd.Utilities.Exceptions
{
    [Serializable]
    public class SqlDatabaseDataException : Exception
    {
        public SqlDatabaseDataException() { }

        public SqlDatabaseDataException(string message) : base(message) { }

        public SqlDatabaseDataException(string message, Exception inner) : base(message, inner) { }
    }
}
