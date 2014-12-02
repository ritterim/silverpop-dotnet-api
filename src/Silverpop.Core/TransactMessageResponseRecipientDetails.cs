using System.Collections.Generic;

namespace Silverpop.Core
{
    public class TransactMessageResponseRecipientDetails
    {
        public string Email { get; set; }

        public TransactMessageResponseRecipientSendStatus SendStatus { get; set; }

        public KeyValuePair<int, string> Error { get; set; }
    }
}