using System.Collections.Generic;

namespace Silverpop.Core
{
    public class TransactMessageRecipient
    {
        public TransactMessageRecipient()
        {
            PersonalizationTags = new Dictionary<string, string>();
        }

        public string EmailAddress { get; set; }

        public TransactMessageRecipientBodyType? BodyType { get; set; }

        public IDictionary<string, string> PersonalizationTags { get; set; }
    }
}