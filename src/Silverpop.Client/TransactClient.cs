using Silverpop.Client.Extensions;
using Silverpop.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    public class TransactClient
    {
        public static readonly string ErrorMissingPodNumber =
            "A valid PodNumber must be provided.";

        public static readonly string ErrorExceededNonBatchRecipients = string.Format(
            "Number of recipients exceeds the max of {0} recipients permitted. " +
            "Use SendMessageBatch or SendMessageBatchAsync instead.",
            TransactClientConfiguration.MaxRecipientsPerNonBatchRequest);

        private readonly TransactClientConfiguration _configuration;
        private readonly TransactMessageEncoder _encoder;
        private readonly TransactMessageResponseDecoder _decoder;
        private readonly Func<ISilverpopCommunicationsClient> _silverpopFactory;

        public TransactClient(TransactClientConfiguration configuration)
            : this(
                configuration,
                new TransactMessageEncoder(),
                new TransactMessageResponseDecoder(),
                () => new SilverpopCommunicationsClient(configuration))
        {
        }

        public TransactClient(
            TransactClientConfiguration configuration,
            TransactMessageEncoder encoder,
            TransactMessageResponseDecoder decoder,
            Func<ISilverpopCommunicationsClient> silverpopFactory)
        {
            _configuration = configuration;
            _encoder = encoder;
            _decoder = decoder;
            _silverpopFactory = silverpopFactory;
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

            string response;
            using (var silverpop = _silverpopFactory())
            {
                response = silverpop.HttpUpload(encodedMessage);
            }

            var decodedResponse = _decoder.Decode(response);

            if (decodedResponse.Status == TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent)
                throw new TransactClientException(
                    decodedResponse.Error.Value, encodedMessage, decodedResponse.RawResponse);

            return decodedResponse;
        }

        public async Task<TransactMessageResponse> SendMessageAsync(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            SendMessagePreCommunicationVerification(message);

            var encodedMessage = _encoder.Encode(message);

            string response;
            using (var silverpop = _silverpopFactory())
            {
                response = await silverpop.HttpUploadAsync(encodedMessage);
            }

            var decodedResponse = _decoder.Decode(response);

            if (decodedResponse.Status == TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent)
                throw new TransactClientException(
                    decodedResponse.Error.Value, encodedMessage, decodedResponse.RawResponse);

            return decodedResponse;
        }

        /// <returns>Filenames can be used for checking status of batches.</returns>
        public IEnumerable<string> SendMessageBatch(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            MessageBatchPreCommunicationVerification();

            var filenames = new List<string>();

            using (var silverpop = _silverpopFactory())
            {
                var batchedMessages = message.GetRecipientBatchedMessages(
                    TransactClientConfiguration.MaxRecipientsPerBatchRequest);

                var startUtc = UtcNow.ToString("s").Replace(':', '_') + "_UTC";

                foreach (var batchMessage in batchedMessages)
                {
                    var encodedMessage = _encoder.Encode(batchMessage);

                    var filename = string.Format(
                        "{0}.{1}.xml.gz",
                        startUtc,
                        filenames.Count() + 1);

                    silverpop.SftpGzipUpload(
                        encodedMessage,
                        "transact/temp/" + filename);

                    silverpop.SftpMove(
                        "transact/temp/" + filename,
                        "transact/inbound/" + filename);

                    filenames.Add(filename);
                }
            }

            return filenames;
        }

        /// <returns>Filenames can be used for checking status of batches.</returns>
        public async Task<IEnumerable<string>> SendMessageBatchAsync(TransactMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            MessageBatchPreCommunicationVerification();

            var filenames = new List<string>();

            using (var silverpop = _silverpopFactory())
            {
                var batchedMessages = message.GetRecipientBatchedMessages(
                    TransactClientConfiguration.MaxRecipientsPerBatchRequest);

                var startUtc = UtcNow.ToString("s").Replace(':', '_') + "_UTC";

                foreach (var batchMessage in batchedMessages)
                {
                    var encodedMessage = _encoder.Encode(batchMessage);

                    var filename = string.Format(
                        "{0}.{1}.xml.gz",
                        startUtc,
                        filenames.Count() + 1);

                    await silverpop.SftpGzipUploadAsync(
                        encodedMessage,
                        "transact/temp/" + filename);

                    await silverpop.SftpMoveAsync(
                        "transact/temp/" + filename,
                        "transact/inbound/" + filename);

                    filenames.Add(filename);
                }
            }

            return filenames;
        }

        public TransactMessageResponse GetStatusOfMessageBatch(string filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");

            MessageBatchPreCommunicationVerification();

            var filePath = "transact/status/" + filename;

            Stream stream;
            using (var silverpop = _silverpopFactory())
            {
                stream = silverpop.SftpDownload(filePath);
            }

            if (stream == null)
                throw new TransactClientException(
                    filename + " does not exist in the transact/status folder",
                    filePath);

            var decodedResponse = _decoder.Decode(stream.ToContentString(Encoding.UTF8));

            return decodedResponse;
        }

        public async Task<TransactMessageResponse> GetStatusOfMessageBatchAsync(string filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");

            MessageBatchPreCommunicationVerification();

            var filePath = "transact/status/" + filename;

            Stream stream;
            using (var silverpop = _silverpopFactory())
            {
                stream = await silverpop.SftpDownloadAsync(filePath);
            }

            if (stream == null)
                throw new TransactClientException(
                    filename + " does not exist in the transact/status folder",
                    filePath);

            var decodedResponse = _decoder.Decode(stream.ToContentString(Encoding.UTF8));

            return decodedResponse;
        }

        private void SendMessagePreCommunicationVerification(TransactMessage message)
        {
            if (message.Recipients.Count() > TransactClientConfiguration.MaxRecipientsPerNonBatchRequest)
                throw new ArgumentException(ErrorExceededNonBatchRecipients);

            if (!_configuration.PodNumber.HasValue)
                throw new ApplicationException(ErrorMissingPodNumber);
        }

        private void MessageBatchPreCommunicationVerification()
        {
            if (!_configuration.PodNumber.HasValue)
                throw new ApplicationException(ErrorMissingPodNumber);
        }
    }
}