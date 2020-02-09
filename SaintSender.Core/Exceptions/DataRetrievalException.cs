using System;

namespace SaintSender.Core.Exceptions
{
    public class DataRetrievalException : Exception
    {
        public DataRetrievalException()
        {
        }

        public DataRetrievalException(string message)
            : base(message)
        {
        }

        public DataRetrievalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
