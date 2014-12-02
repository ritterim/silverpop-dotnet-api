using System.Collections.Generic;

namespace Silverpop.Core
{
    public class TransactMessage
    {
        public TransactMessage()
        {
            SaveColumns = new List<string>();
            Recipients = new List<TransactMessageRecipient>();
        }

        public string CampaignId { get; set; }

        public string TransactionId { get; set; }

        public bool ShowAllSendDetail { get; set; }

        public bool SendAsBatch { get; set; }

        public bool NoRetryOnFailure { get; set; }

        public IEnumerable<string> SaveColumns { get; set; }

        public IEnumerable<TransactMessageRecipient> Recipients { get; set; }
    }
}