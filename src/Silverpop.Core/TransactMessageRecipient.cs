using Silverpop.Core.Extensions;
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
            PersonalizationTags = new Dictionary<string, string>();
        }

        public string EmailAddress { get; set; }

        public TransactMessageRecipientBodyType? BodyType { get; set; }

        public IDictionary<string, string> PersonalizationTags { get; set; }

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

            var personalizationTags = GetPersonalizationTagsDictionary(personalizationTagsObject);

            if (propertiesToUse != null)
            {
                personalizationTags = personalizationTags
                    .Where(x => propertiesToUse.Any(y => y == x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
            }

            return new TransactMessageRecipient()
            {
                EmailAddress = emailAddress,
                BodyType = bodyType,
                PersonalizationTags = personalizationTags
            };
        }

        // Adapted from: http://stackoverflow.com/a/4944547/941536
        private static IDictionary<string, string> GetPersonalizationTagsDictionary(
            object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            var dictionary = new Dictionary<string, string>();

            var properties = source.GetType().GetProperties(bindingAttr);
            foreach (var property in properties)
            {
                var personalizationTagAttribute =
                    property.GetCustomAttribute<SilverpopPersonalizationTag>();

                dictionary.Add(
                    personalizationTagAttribute != null
                        ? personalizationTagAttribute.Name
                        : property.Name,
                    property.GetValue(source, null).ToString());
            }

            return dictionary;
        }
    }
}