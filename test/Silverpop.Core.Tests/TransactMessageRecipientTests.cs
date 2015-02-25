using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Silverpop.Core.Tests
{
    public class TransactMessageRecipientTests
    {
        public class CreateTests
        {
            [Fact]
            public void ThrowsForNullEmailAddress()
            {
                Assert.Throws<ArgumentNullException>(
                    () => TransactMessageRecipient.Create(null));
            }

            [Fact]
            public void SetsEmailAddress()
            {
                var recipient = TransactMessageRecipient.Create(
                    "test@example.com");

                Assert.Equal("test@example.com", recipient.EmailAddress);
            }

            [Fact]
            public void SetsBodyType()
            {
                var recipient = TransactMessageRecipient.Create(
                    "test@example.com",
                    bodyType: TransactMessageRecipientBodyType.Text);

                Assert.Equal(TransactMessageRecipientBodyType.Text, recipient.BodyType);
            }

            [Fact]
            public void SetsPersonalizationTags()
            {
                var recipient = TransactMessageRecipient.Create(
                    "test@example.com",
                    personalizationTags: new Dictionary<string, string>()
                    {
                        { "tag1", "tag1-value" },
                        { "tag2", "tag2-value" }
                    });

                Assert.Equal(2, recipient.PersonalizationTags.Count());

                Assert.Equal("tag1", recipient.PersonalizationTags.First().Key);
                Assert.Equal("tag1-value", recipient.PersonalizationTags.First().Value);

                Assert.Equal("tag2", recipient.PersonalizationTags.Last().Key);
                Assert.Equal("tag2-value", recipient.PersonalizationTags.Last().Value);
            }
        }
    }
}