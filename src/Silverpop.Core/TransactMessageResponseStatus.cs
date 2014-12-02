namespace Silverpop.Core
{
    public enum TransactMessageResponseStatus
    {
        /// <summary>
        /// 0 – Did not encounter any errors during the send;
        /// all recipients sent.
        /// </summary>
        NoErrorsAllRecipientsSent = 0,

        /// <summary>
        /// 1 – Encountered errors during the send;
        /// some or all recipients were not sent
        /// </summary>
        EncounteredErrorsPossiblySomeMessagesSent = 1,

        /// <summary>
        /// 2 – Encountered errors with the XML submission;
        /// no recipients were sent;
        /// no recipient error details provided, only the request-level error
        /// </summary>
        EncounteredErrorsNoMessagesSent = 2,
    }
}