using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Silverpop.Core
{
    public class TransactMessageEncoder
    {
        public string Encode(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            var xml = new XElement(XName.Get("XTMAILING"));

            xml.SetElementValue(XName.Get("CAMPAIGN_ID"), message.CampaignId);
            xml.SetElementValue(XName.Get("TRANSACTION_ID"), message.TransactionId);
            xml.SetElementValue(XName.Get("SHOW_ALL_SEND_DETAIL"), message.ShowAllSendDetail);
            xml.SetElementValue(XName.Get("SEND_AS_BATCH"), message.SendAsBatch);
            xml.SetElementValue(XName.Get("NO_RETRY_ON_FAILURE"), message.NoRetryOnFailure);

            // Add SAVE_COLUMNS
            var saveColumnsXml = new XElement(XName.Get("SAVE_COLUMNS"));
            foreach (var saveColumn in message.SaveColumns)
            {
                var columnName = new XElement(XName.Get("COLUMN_NAME"))
                {
                    Value = saveColumn
                };

                saveColumnsXml.Add(columnName);
            }
            xml.Add(saveColumnsXml);

            // Add RECIPIENT nodes
            foreach (var recipient in message.Recipients)
            {
                var recipientXml = new XElement(XName.Get("RECIPIENT"));

                recipientXml.SetElementValue(XName.Get("EMAIL"), recipient.EmailAddress);
                recipientXml.SetElementValue(XName.Get("BODY_TYPE"), recipient.BodyType.ToString().ToUpper());

                // Add PERSONALIZATION nodes for RECIPIENT
                foreach (var personalizationTag in recipient.PersonalizationTags)
                {
                    var personalizationXml = new XElement(XName.Get("PERSONALIZATION"));

                    personalizationXml.SetElementValue(XName.Get("TAG_NAME"), personalizationTag.Key);

                    // Prevent usage of XML CDATA sections,
                    // if there is reason to allow these it can be revisited.
                    if (ContainsCDATASection(personalizationTag.Value))
                        throw new ArgumentException(
                            "XML CDATA sections should not be used in PersonalizationTags values.");

                    personalizationXml.SetElementValue(XName.Get("VALUE"), personalizationTag.Value);

                    recipientXml.Add(personalizationXml);
                }

                xml.Add(recipientXml);
            }

            return xml.ToString();
        }

        private static bool ContainsCDATASection(string str)
        {
            return Regex.Match(str, @"<!\[CDATA\[(.|\n|\r)*]]>").Success;
        }
    }
}