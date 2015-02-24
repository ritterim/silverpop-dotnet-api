using Silverpop.Core.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Xunit;

namespace Silverpop.Core.Tests
{
    public class TransactMessageEncoderTests
    {
        public class EncodeMethodTests
        {
            private const string RecipientRegex = "<RECIPIENT>(\n|\r|.)*?<\\/RECIPIENT>";

            private static string EncodedMessage(
                string campaignId = "001",
                string transactionId = null,
                bool showAllSendDetail = false,
                bool sendAsBatch = false,
                bool noRetryOnFailure = false,
                ICollection<string> saveColumns = null,
                ICollection<TransactMessageRecipient> recipients = null)
            {
                if (saveColumns == null)
                    saveColumns = new List<string>();

                if (recipients == null)
                    recipients = new List<TransactMessageRecipient>();

                var message = new TransactMessage()
                {
                    CampaignId = campaignId,
                    TransactionId = transactionId,
                    ShowAllSendDetail = showAllSendDetail,
                    SendAsBatch = sendAsBatch,
                    NoRetryOnFailure = noRetryOnFailure,
                    SaveColumns = saveColumns,
                    Recipients = recipients,
                };

                return new TransactMessageEncoder().Encode(message);
            }

            [Fact]
            public void ThrowsIfNullMessage()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new TransactMessageEncoder().Encode(null));
            }

            [Fact]
            public void IsValidXml()
            {
                Assert.DoesNotThrow(
                    () => new XmlDocument().LoadXml(EncodedMessage()));
            }

            [Fact]
            public void HasCorrectRootNode()
            {
                Assert.True(EncodedMessage()
                    .StartsWith("<XTMAILING>"));
            }

            [Fact]
            public void EncodesCampaignId()
            {
                Assert.True(EncodedMessage(campaignId: "12345")
                    .Contains("<CAMPAIGN_ID>12345</CAMPAIGN_ID>"));
            }

            [Fact]
            public void EncodesTransactionId()
            {
                Assert.True(EncodedMessage(transactionId: "12345")
                    .Contains("<TRANSACTION_ID>12345</TRANSACTION_ID>"));
            }

            [Fact]
            public void EncodesDefaultTransactionIdWhenNull()
            {
                Assert.True(Regex.Match(EncodedMessage(),
                    @"<TRANSACTION_ID>dotnet-api-.{36}<\/TRANSACTION_ID>").Success);
            }

            [Fact]
            public void EncodesShowAllSendDetail()
            {
                Assert.True(EncodedMessage(showAllSendDetail: true)
                    .Contains("<SHOW_ALL_SEND_DETAIL>true</SHOW_ALL_SEND_DETAIL>"));
            }

            [Fact]
            public void EncodesSendAsBatch()
            {
                Assert.True(EncodedMessage(sendAsBatch: true)
                    .Contains("<SEND_AS_BATCH>true</SEND_AS_BATCH>"));
            }

            [Fact]
            public void EncodesNoRetryOnFailure()
            {
                Assert.True(EncodedMessage(noRetryOnFailure: true)
                    .Contains("<NO_RETRY_ON_FAILURE>true</NO_RETRY_ON_FAILURE>"));
            }

            [Fact]
            public void EncodesSaveColumns()
            {
                Assert.True(EncodedMessage(saveColumns: new List<string>() { "column1", "column2" })
                    .ContainsIgnoreWhitespaceAndNewLines(
                        "<SAVE_COLUMNS><COLUMN_NAME>column1</COLUMN_NAME><COLUMN_NAME>column2</COLUMN_NAME>"));
            }

            [Fact]
            public void EncodesRecipients()
            {
                Assert.Equal(
                    2,
                    Regex.Matches(
                        EncodedMessage(recipients: new List<TransactMessageRecipient>()
                        {
                            new TransactMessageRecipient() { EmailAddress = "test1@example.com" },
                            new TransactMessageRecipient() { EmailAddress = "test2@example.com" },
                        }), RecipientRegex).Count);
            }

            public class EncodesRecipientTests
            {
                private static string RecipientTag = Regex.Match(
                    EncodedMessage(recipients: new List<TransactMessageRecipient>()
                    {
                        new TransactMessageRecipient()
                        {
                            EmailAddress = "test1@example.com",
                            BodyType = TransactMessageRecipientBodyType.Text,
                            PersonalizationTags = new Dictionary<string, string>()
                            {
                                { "tag1", "tag1-value" },
                                { "tag2", "tag2-value" },
                            }
                        }
                    }), RecipientRegex).Value;

                [Fact]
                public void Email()
                {
                    Assert.True(RecipientTag
                        .Contains("<EMAIL>test1@example.com</EMAIL>"));
                }

                [Fact]
                public void BodyType()
                {
                    Assert.True(RecipientTag
                        .Contains("<BODY_TYPE>TEXT</BODY_TYPE>"));
                }

                [Fact]
                public void PersonalizationTags_EncodesAll()
                {
                    Assert.Equal(
                        2,
                        Regex.Matches(RecipientTag, Regex.Escape("<PERSONALIZATION>")).Count);
                }

                [Fact]
                public void PersonalizationTags_TagName()
                {
                    Assert.True(RecipientTag
                        .ContainsIgnoreWhitespaceAndNewLines(
                            "<PERSONALIZATION><TAG_NAME>tag1</TAG_NAME>"));
                }

                [Fact]
                public void PersonalizationTags_Value()
                {
                    Assert.True(RecipientTag
                        .ContainsIgnoreWhitespaceAndNewLines(
                            "<VALUE>tag2-value</VALUE></PERSONALIZATION>"));
                }

                [Fact]
                public void PersonalizationTags_Value_EncodesCharacters()
                {
                    var recipientTag = Regex.Match(
                        EncodedMessage(recipients: new List<TransactMessageRecipient>()
                        {
                            new TransactMessageRecipient()
                            {
                                EmailAddress = "test1@example.com",
                                BodyType = TransactMessageRecipientBodyType.Text,
                                PersonalizationTags = new Dictionary<string, string>()
                                {
                                    { "tag1&", "<value>" },
                                }
                            }
                        }), RecipientRegex).Value;

                    Assert.True(recipientTag
                        .ContainsIgnoreWhitespaceAndNewLines(
                            "<TAG_NAME>tag1&amp;</TAG_NAME><VALUE>&lt;value&gt;</VALUE>"));
                }

                [Fact]
                public void PersonalizationTags_Value_ThrowsWhenCDATASectionIsUsed()
                {
                    var exception = Assert.Throws<ArgumentException>(
                        () => EncodedMessage(recipients: new List<TransactMessageRecipient>()
                              {
                                  new TransactMessageRecipient()
                                  {
                                      EmailAddress = "test1@example.com",
                                      BodyType = TransactMessageRecipientBodyType.Html,
                                      PersonalizationTags = new Dictionary<string, string>()
                                      {
                                          {
                                              "tag1",
                                              "<![CDATA[<html><body>" +
                                                  Environment.NewLine +
                                                  @"<a href=""http://"">test</a></body></html>]]>"
                                          },
                                      }
                                  }
                              }));

                    Assert.Equal(
                        "XML CDATA sections should not be used in PersonalizationTags values.",
                        exception.Message);
                }
            }
        }
    }
}