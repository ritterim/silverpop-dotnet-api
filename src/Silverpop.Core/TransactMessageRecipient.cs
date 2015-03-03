using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silverpop.Core
{
    public class TransactMessageRecipient
    {
        public TransactMessageRecipient()
        {
            PersonalizationTags = new List<TransactMessageRecipientPersonalizationTag>();
        }

        public string EmailAddress { get; set; }

        public TransactMessageRecipientBodyType? BodyType { get; set; }

        public IEnumerable<TransactMessageRecipientPersonalizationTag> PersonalizationTags { get; set; }

        public static TransactMessageRecipient Create(
            string emailAddress,
            TransactMessageRecipientBodyType? bodyType = TransactMessageRecipientBodyType.Html)
        {
            if (emailAddress == null) throw new ArgumentNullException("emailAddress");

            return new TransactMessageRecipient()
            {
                EmailAddress = emailAddress,
                BodyType = bodyType
            };
        }

        /// <param name="propertiesToUse">Optional. If not present, will use all available properties.</param>
        public static TransactMessageRecipient Create<T>(
            string emailAddress,
            T personalizationTagsObject,
            IEnumerable<string> propertiesToUse = null,
            TransactMessageRecipientBodyType? bodyType = TransactMessageRecipientBodyType.Html)
        {
            if (emailAddress == null) throw new ArgumentNullException("emailAddress");
            if (personalizationTagsObject == null) throw new ArgumentNullException("personalizationTagsObject");

            var personalizationTags =
                GetTransactMessageRecipientPersonalizationTags(personalizationTagsObject);

            if (propertiesToUse != null)
            {
                personalizationTags = personalizationTags
                    .Where(x => propertiesToUse.Any(y => y == x.Name));
            }

            return new TransactMessageRecipient()
            {
                EmailAddress = emailAddress,
                BodyType = bodyType,
                PersonalizationTags = personalizationTags
            };
        }

        // Adapted from: http://stackoverflow.com/a/4944547/941536
        private static IEnumerable<TransactMessageRecipientPersonalizationTag> GetTransactMessageRecipientPersonalizationTags(
            object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            var properties = source.GetType().GetProperties(bindingAttr);
            foreach (var property in properties)
            {
                var personalizationTagAttribute =
                    property.GetCustomAttribute<SilverpopPersonalizationTag>();

                yield return new TransactMessageRecipientPersonalizationTag(
                    personalizationTagAttribute != null
                        ? personalizationTagAttribute.Name
                        : property.Name,
                    property.GetValue(source, null).ToString());
            }
        }
    }
}