using Silverpop.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public ICollection<TransactMessageRecipient> Recipients { get; set; }

        public IEnumerable<TransactMessage> GetRecipientBatchedMessages(int maxRecipientsPerMessage)
        {
            if (maxRecipientsPerMessage <= 0)
                throw new ArgumentOutOfRangeException("maxRecipientsPerMessage");

            var messages = new List<TransactMessage>();
            foreach (var recipientsBatch in this.Recipients.Batch(maxRecipientsPerMessage))
            {
                var batchMessage = CloneWithoutRecipients(this);
                batchMessage.Recipients = recipientsBatch.ToList();

                messages.Add(batchMessage);
            }

            return messages;
        }

        /// <remarks>
        /// I'm not a huge fan of this manual cloning.
        /// However, I'm choosing this over taking a dependency on a mapper
        /// or performing a deep clone that includes the Recipients collection unnecessarily.
        /// </remarks>
        public static TransactMessage CloneWithoutRecipients(TransactMessage message)
        {
            return new TransactMessage()
            {
                CampaignId = message.CampaignId,
                TransactionId = message.TransactionId,
                ShowAllSendDetail = message.ShowAllSendDetail,
                SendAsBatch = message.SendAsBatch,
                NoRetryOnFailure = message.NoRetryOnFailure,
                SaveColumns = message.SaveColumns,
            };
        }
    }
}