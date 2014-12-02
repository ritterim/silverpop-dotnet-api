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

        /// <summary>
        /// Dictionary values should be wrapped in <![CDATA[ ... ]]> if necessary.
        /// </summary>
        public IDictionary<string, string> PersonalizationTags { get; set; }
    }
}