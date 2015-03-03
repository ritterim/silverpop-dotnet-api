using Silverpop.Core;
using System.Collections.Generic;

namespace Silverpop.Client.WebTester.Models
{
    public class SendModel
    {
        public SendModel()
        {
            PersonalizationTags = new List<TransactMessageRecipientPersonalizationTag>();
        }

        public string CampaignId { get; set; }

        public string ToAddress { get; set; }

        public IEnumerable<TransactMessageRecipientPersonalizationTag> PersonalizationTags { get; set; }
    }
}