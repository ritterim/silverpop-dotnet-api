namespace Silverpop.Core
{
    public enum TransactMessageResponseRecipientSendStatus
    {
        /// <summary>
        /// 0 – no errors encountered during the send.
        /// </summary>
        NoErrorsEncounteredDuringSend = 0,

        /// <summary>
        /// 1 – Encountered an error during the send, will not retry the send.
        /// </summary>
        ErrorEncounteredWillNotRetry = 1,

        /// <summary>
        /// 2 – request received; send cached for later send.
        /// </summary>
        RequestReceivedSendCachedForLaterSend = 2,
    }
}