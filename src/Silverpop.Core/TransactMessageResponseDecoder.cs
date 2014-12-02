using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Silverpop.Core
{
    public class TransactMessageResponseDecoder
    {
        public virtual TransactMessageResponse Decode(string xmlResponse)
        {
            if (xmlResponse == null) throw new ArgumentNullException("xmlResponse");

            var xml = XElement.Parse(xmlResponse);

            if (xml.Name != "XTMAILING_RESPONSE")
                throw new ArgumentException("xmlResponse must have a root XTMAILING_RESPONSE node.");

            return new TransactMessageResponse()
            {
                RawResponse = xmlResponse,
                CampaignId = xml.Element(XName.Get("CAMPAIGN_ID")).Value,
                TransactionId = xml.Element(XName.Get("TRANSACTION_ID")).Value,
                RecipientsReceived = Convert.ToInt32(xml.Element(XName.Get("RECIPIENTS_RECEIVED")).Value),
                EmailsSent = Convert.ToInt32(xml.Element(XName.Get("EMAILS_SENT")).Value),
                Status = (TransactMessageResponseStatus)
                    Enum.Parse(
                        typeof(TransactMessageResponseStatus),
                        xml.Element(XName.Get("STATUS")).Value),
                Error = new KeyValuePair<int, string>(
                            Convert.ToInt32(xml.Element(XName.Get("ERROR_CODE")).Value),
                            xml.Element(XName.Get("ERROR_STRING")).Value),
                RecipientDetails = xml.Elements(XName.Get("RECIPIENT_DETAIL"))
                    .Select(x => new TransactMessageResponseRecipientDetails()
                    {
                        Email = x.Element(XName.Get("EMAIL")).Value,
                        SendStatus = (TransactMessageResponseRecipientSendStatus)
                            Enum.Parse(
                                typeof(TransactMessageResponseRecipientSendStatus),
                                x.Element(XName.Get("SEND_STATUS")).Value),
                        Error = new KeyValuePair<int, string>(
                            Convert.ToInt32(x.Element(XName.Get("ERROR_CODE")).Value),
                            x.Element(XName.Get("ERROR_STRING")).Value)
                    })
            };
        }
    }
}