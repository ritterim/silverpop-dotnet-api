using System;

namespace Silverpop.Client
{
    public class TransactClientException : Exception
    {
        public TransactClientException()
        {
        }

        public TransactClientException(string message)
            : base(message)
        {
        }

        public TransactClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}