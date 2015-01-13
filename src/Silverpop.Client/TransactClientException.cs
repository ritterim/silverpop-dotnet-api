using System;

namespace Silverpop.Client
{
    [Serializable]
    public class TransactClientException : Exception
    {
        public TransactClientException(string message, string request)
            : base(message)
        {
            Request = request;
        }

        public TransactClientException(string message, string request, string response)
            : base(message)
        {
            Request = request;
            Response = response;
        }

        public string Request { get; private set; }

        public string Response { get; private set; }
    }
}