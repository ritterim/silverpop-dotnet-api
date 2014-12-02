using System;
using System.Linq;
using Xunit;

namespace Silverpop.Core.Tests
{
    public class TransactMessageResponseDecoderTests
    {
        public class DecodeMethodTests
        {
            private static readonly string TestResponseString =
@"<XTMAILING_RESPONSE>
    <CAMPAIGN_ID>300401</CAMPAIGN_ID>
    <TRANSACTION_ID>TRANS-1234</TRANSACTION_ID>
    <RECIPIENTS_RECEIVED>1</RECIPIENTS_RECEIVED>
    <EMAILS_SENT>0</EMAILS_SENT>
    <NUMBER_ERRORS>{0}</NUMBER_ERRORS>
    <STATUS>{1}</STATUS>
    <ERROR_CODE>{2}</ERROR_CODE>
    <ERROR_STRING>{3}</ERROR_STRING>
    <RECIPIENT_DETAIL>
        <EMAIL>test@example.com</EMAIL>
        <SEND_STATUS>{4}</SEND_STATUS>
        <ERROR_CODE>{5}</ERROR_CODE>
        <ERROR_STRING>{6}</ERROR_STRING>
    </RECIPIENT_DETAIL>
</XTMAILING_RESPONSE>";

            private static TransactMessageResponse DecodedResponse(
                int status = 0,
                int errorCode = 0,
                string errorString = null,
                int recipientDetailSendStatus = 0,
                int recipientDetailErrorCode = 0,
                string recipientDetailErrorString = null)
            {
                var numberOfErrors = 0;

                if (errorCode != 0)
                    numberOfErrors++;

                if (recipientDetailErrorCode != 0)
                    numberOfErrors++;

                var xmlString = string.Format(
                    TestResponseString,
                    numberOfErrors,
                    status,
                    errorCode,
                    errorString,
                    recipientDetailSendStatus,
                    recipientDetailErrorCode,
                    recipientDetailErrorString);

                return new TransactMessageResponseDecoder().Decode(xmlString);
            }

            [Fact]
            public void ThrowsIfNullResponse()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new TransactMessageResponseDecoder().Decode(null));
            }

            [Fact]
            public void ThrowsForUnexpectedResponse()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new TransactMessageResponseDecoder().Decode("<html><body>Error!</body></html>"));

                Assert.Equal(
                    "xmlResponse must have a root XTMAILING_RESPONSE node.",
                    exception.Message);
            }

            [Fact]
            public void DecodesRawResponse()
            {
                var expectedRawResponse = string.Format(
                    TestResponseString,
                    /* numberOfErrors */ 0,
                    /* status */ 0,
                    /* errorCode */ 0,
                    /* errorString */ null,
                    /* recipientDetailSendStatus */ 0,
                    /* recipientDetailErrorCode */ 0,
                    /* recipientDetailErrorString */ null);

                Assert.Equal(
                    expectedRawResponse,
                    DecodedResponse().RawResponse);
            }

            [Fact]
            public void DecodesCampaignId()
            {
                Assert.Equal(
                    "300401",
                    DecodedResponse().CampaignId);
            }

            [Fact]
            public void DecodesTransactionId()
            {
                Assert.Equal(
                    "TRANS-1234",
                    DecodedResponse().TransactionId);
            }

            [Fact]
            public void DecodesRecipientsReceived()
            {
                Assert.Equal(
                    1,
                    DecodedResponse().RecipientsReceived);
            }

            [Fact]
            public void DecodesEmailsSent()
            {
                Assert.Equal(
                    0,
                    DecodedResponse().EmailsSent);
            }

            [Fact]
            public void DecodesStatus0()
            {
                Assert.Equal(
                    TransactMessageResponseStatus.NoErrorsAllRecipientsSent,
                    DecodedResponse(status: 0).Status);
            }

            [Fact]
            public void DecodesStatus1()
            {
                Assert.Equal(
                    TransactMessageResponseStatus.EncounteredErrorsPossiblySomeMessagesSent,
                    DecodedResponse(status: 1).Status);
            }

            [Fact]
            public void DecodesStatus2()
            {
                Assert.Equal(
                    TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent,
                    DecodedResponse(status: 2).Status);
            }

            [Fact]
            public void DecodesErrorCode()
            {
                Assert.Equal(
                    123,
                    DecodedResponse(errorCode: 123).Error.Key);
            }

            [Fact]
            public void DecodesErrorString()
            {
                Assert.Equal(
                    "test error string",
                    DecodedResponse(errorString: "test error string").Error.Value);
            }

            [Fact]
            public void DecodesRecipientDetailEmail()
            {
                Assert.Equal(
                    "test@example.com",
                    DecodedResponse().RecipientDetails.Single().Email);
            }

            [Fact]
            public void DecodesRecipientDetailSendStatus0()
            {
                Assert.Equal(
                    TransactMessageResponseRecipientSendStatus.NoErrorsEncounteredDuringSend,
                    DecodedResponse(recipientDetailSendStatus: 0).RecipientDetails.Single().SendStatus);
            }

            [Fact]
            public void DecodesRecipientDetailSendStatus1()
            {
                Assert.Equal(
                    TransactMessageResponseRecipientSendStatus.ErrorEncounteredWillNotRetry,
                    DecodedResponse(recipientDetailSendStatus: 1).RecipientDetails.Single().SendStatus);
            }

            [Fact]
            public void DecodesRecipientDetailSendStatus2()
            {
                Assert.Equal(
                    TransactMessageResponseRecipientSendStatus.RequestReceivedSendCachedForLaterSend,
                    DecodedResponse(recipientDetailSendStatus: 2).RecipientDetails.Single().SendStatus);
            }

            [Fact]
            public void DecodesRecipientDetailErrorCode()
            {
                Assert.Equal(
                    123,
                    DecodedResponse(recipientDetailErrorCode: 123)
                        .RecipientDetails.Single().Error.Key);
            }

            [Fact]
            public void DecodesRecipientDetailErrorString()
            {
                Assert.Equal(
                    "test recipient error",
                    DecodedResponse(recipientDetailErrorString: "test recipient error")
                        .RecipientDetails.Single().Error.Value);
            }
        }
    }
}