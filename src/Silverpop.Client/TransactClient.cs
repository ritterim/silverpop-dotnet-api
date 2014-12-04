using Silverpop.Client.Extensions;
using Silverpop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    public class TransactClient
    {
        public const int MaxRecipientsPerBatchRequest = 5000;
        public const int MaxRecipientsPerNonBatchRequest = 10;

        public static readonly string ErrorMissingHttpUrl =
            "A valid TransactHttpUrl must be provided.";

        public static readonly string ErrorMissingFtpUrl =
            "A valid TransactFtpUrl must be provided.";

        public static readonly string ErrorExceededNonBatchRecipients = string.Format(
            "Number of recipients exceeds the max of {0} recipients permitted. " +
            "Use SendMessageBatch or SendMessageBatchAsync instead.",
            MaxRecipientsPerNonBatchRequest);

        private readonly TransactClientConfiguration _configuration;
        private readonly TransactMessageEncoder _encoder;
        private readonly TransactMessageResponseDecoder _decoder;
        private readonly ISilverpopCommunicationsClient _silverpop;

        public TransactClient(TransactClientConfiguration configuration)
            : this(
                configuration,
                new TransactMessageEncoder(),
                new TransactMessageResponseDecoder(),
                new SilverpopCommunicationsClient(configuration))
        {
        }

        public TransactClient(
            TransactClientConfiguration configuration,
            TransactMessageEncoder encoder,
            TransactMessageResponseDecoder decoder,
            ISilverpopCommunicationsClient silverpop)
        {
            _configuration = configuration;
            _encoder = encoder;
            _decoder = decoder;
            _silverpop = silverpop;
        }

        public virtual DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }

        public TransactMessageResponse SendMessage(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            SendMessagePreCommunicationVerification(message);

            var encodedMessage = _encoder.Encode(message);
            var response = _silverpop.HttpUpload(encodedMessage);
            var decodedResponse = _decoder.Decode(response);

            if (decodedResponse.Status == TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent)
                throw new TransactClientException(decodedResponse.Error.Value);

            return decodedResponse;
        }

        public async Task<TransactMessageResponse> SendMessageAsync(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            SendMessagePreCommunicationVerification(message);

            var encodedMessage = _encoder.Encode(message);
            var response = await _silverpop.HttpUploadAsync(encodedMessage);
            var decodedResponse = _decoder.Decode(response);

            if (decodedResponse.Status == TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent)
                throw new TransactClientException(decodedResponse.Error.Value);

            return decodedResponse;
        }

        /// <returns>Filenames can be used for checking status of batches.</returns>
        public IEnumerable<string> SendMessageBatch(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            MessageBatchPreCommunicationVerification();

            var filenames = new List<string>();
            foreach (var batchMessage in message.GetRecipientBatchedMessages(MaxRecipientsPerBatchRequest))
            {
                var encodedMessage = _encoder.Encode(batchMessage);

                var filename = string.Format("{0}_UTC.{1}.xml",
                    UtcNow.ToString("s").Replace(':', '_'),
                    filenames.Count() + 1);

                _silverpop.FtpUpload(
                    encodedMessage,
                    "transact/temp/" + filename);

                _silverpop.FtpMove(
                    "transact/temp/" + filename,
                    "transact/inbound/" + filename);

                filenames.Add(filename);
            }

            return filenames;
        }

        /// <returns>Filenames can be used for checking status of batches.</returns>
        public async Task<IEnumerable<string>> SendMessageBatchAsync(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            MessageBatchPreCommunicationVerification();

            var filenames = new List<string>();
            foreach (var batchMessage in message.GetRecipientBatchedMessages(MaxRecipientsPerBatchRequest))
            {
                var encodedMessage = _encoder.Encode(batchMessage);

                var filename = string.Format("{0}_UTC.{1}.xml",
                    UtcNow.ToString("s").Replace(':', '_'),
                    filenames.Count() + 1);

                await _silverpop.FtpUploadAsync(
                    encodedMessage,
                    "transact/temp/" + filename);

                _silverpop.FtpMove(
                    "transact/temp/" + filename,
                    "transact/inbound/" + filename);

                filenames.Add(filename);
            }

            return filenames;
        }

        public TransactMessageResponse GetStatusOfMessageBatch(string filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");

            MessageBatchPreCommunicationVerification();

            var stream = _silverpop.FtpDownload("transact/status/" + filename);
            if (stream == null)
                throw new TransactClientException(
                    filename + " does not exist in the transact/status folder");

            var decodedResponse = _decoder.Decode(stream.ToContentString(Encoding.UTF8));

            return decodedResponse;
        }

        public async Task<TransactMessageResponse> GetStatusOfMessageBatchAsync(string filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");

            MessageBatchPreCommunicationVerification();

            var stream = await _silverpop.FtpDownloadAsync("transact/status/" + filename);
            if (stream == null)
                throw new TransactClientException(
                    filename + " does not exist in the transact/status folder");

            var decodedResponse = _decoder.Decode(stream.ToContentString(Encoding.UTF8));

            return decodedResponse;
        }

        private void SendMessagePreCommunicationVerification(TransactMessage message)
        {
            if (message.Recipients.Count() > MaxRecipientsPerNonBatchRequest)
                throw new ArgumentException(ErrorExceededNonBatchRecipients);

            if (string.IsNullOrWhiteSpace(_configuration.TransactHttpHost))
                throw new ApplicationException(ErrorMissingHttpUrl);
        }

        private void MessageBatchPreCommunicationVerification()
        {
            if (string.IsNullOrWhiteSpace(_configuration.TransactFtpHost))
                throw new ApplicationException(ErrorMissingFtpUrl);
        }
    }
}