using System.Collections.Generic;

namespace Silverpop.Core
{
    public class TransactMessageResponse
    {
        public TransactMessageResponse()
        {
            RecipientDetails = new List<TransactMessageResponseRecipientDetails>();
        }

        public string RawResponse { get; set; }

        public string CampaignId { get; set; }

        public string TransactionId { get; set; }

        public int RecipientsReceived { get; set; }

        public int EmailsSent { get; set; }

        public TransactMessageResponseStatus? Status { get; set; }

        public KeyValuePair<int, string> Error { get; set; }

        public IEnumerable<TransactMessageResponseRecipientDetails> RecipientDetails { get; set; }
    }
}