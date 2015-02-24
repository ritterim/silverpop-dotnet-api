using System;
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

        public static TransactMessageRecipient Create(
            string emailAddress,
            TransactMessageRecipientBodyType? bodyType = TransactMessageRecipientBodyType.Html,
            IDictionary<string, string> personalizationTags = null)
        {
            if (emailAddress == null) throw new ArgumentNullException("emailAddress");

            return new TransactMessageRecipient()
            {
                EmailAddress = emailAddress,
                BodyType = bodyType,
                PersonalizationTags = personalizationTags ?? new Dictionary<string, string>()
            };
        }
    }
}