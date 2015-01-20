using AssertExLib;
using Moq;
using Silverpop.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using Xunit.Extensions;

namespace Silverpop.Client.Tests
{
    public class TransactClientTests
    {
        protected static readonly ICollection<TransactMessageRecipient> TestRecipients =
            Enumerable.Range(0, 1)
                .Select(x => new TransactMessageRecipient()
                {
                    EmailAddress = Guid.NewGuid().ToString() + "@example.com"
                }).ToList();

        protected static readonly ICollection<TransactMessageRecipient> TestRecipientsTwoBatches =
            Enumerable.Range(0, TransactClientConfiguration.MaxRecipientsPerBatchRequest + 1)
                .Select(x => new TransactMessageRecipient())
                .ToList();

        public class SendMessageMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessage()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new TransactClientTester().SendMessage(null));
            }

            [Fact]
            public void ThrowsForTooManyRecipients()
            {
                var message = new TransactMessage()
                {
                    Recipients = Enumerable.Range(0, TransactClientConfiguration.MaxRecipientsPerNonBatchRequest + 1)
                        .Select(x => new TransactMessageRecipient())
                        .ToList()
                };

                var exception = Assert.Throws<ArgumentException>(
                    () => new TransactClientTester().SendMessage(message));

                Assert.Equal(TransactClient.ErrorExceededNonBatchRecipients, exception.Message);
            }

            [Fact]
            public void ThrowsWhenPodNumberIsNotConfigured()
            {
                var exception = Assert.Throws<ApplicationException>(
                    () => new TransactClientTester().SendMessage(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingPodNumber, exception.Message);
            }

            [Fact]
            public void ThrowsWhenResponseIndicatesEncounteredErrorsNoMessagesSent()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse()
                    {
                        Status = TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent,
                        Error = new KeyValuePair<int, string>(1, "An error occurred."),
                        RawResponse = "test-response"
                    });

                var exception = Assert.Throws<TransactClientException>(
                    () => new TransactClientTester(configuration: new TransactClientConfiguration()
                          {
                              PodNumber = 0
                          }, decoder: decoder).SendMessage(new TransactMessage()));

                Assert.Equal("An error occurred.", exception.Message);
                Assert.Equal("XTMAILING", XDocument.Parse(exception.Request).Root.Name);
                Assert.Equal("test-response", exception.Response);
            }

            [Fact]
            public void PerformsHttpRequest()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).SendMessage(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(
                        x => x.HttpUpload(It.IsAny<string>(), /* tryRefreshingOAuthAccessToken: */ true),
                        Times.Once());
            }

            [Fact]
            public void ReturnsDecodedResponse()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse()
                    {
                        TransactionId = "123"
                    });

                var response = new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder).SendMessage(new TransactMessage());

                Assert.Equal("123", response.TransactionId);
            }

            [Fact]
            public void DisposesISilverpopCommunicationsClient()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).SendMessage(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(x => x.Dispose(), Times.Once());
            }
        }

        public class SendMessageAsyncMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessage()
            {
                AssertEx.TaskThrows<ArgumentNullException>(
                    () => new TransactClientTester().SendMessageAsync(null));
            }

            [Fact]
            public void ThrowsForTooManyRecipients()
            {
                var message = new TransactMessage()
                {
                    Recipients = Enumerable.Range(0, TransactClientConfiguration.MaxRecipientsPerNonBatchRequest + 1)
                        .Select(x => new TransactMessageRecipient())
                        .ToList()
                };

                var exception = AssertEx.TaskThrows<ArgumentException>(
                    () => new TransactClientTester().SendMessageAsync(message));

                Assert.Equal(TransactClient.ErrorExceededNonBatchRecipients, exception.Message);
            }

            [Fact]
            public void ThrowsWhenPodNumberIsNotConfigured()
            {
                var exception = AssertEx.TaskThrows<ApplicationException>(
                    () => new TransactClientTester().SendMessageAsync(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingPodNumber, exception.Message);
            }

            [Fact]
            public void ThrowsWhenResponseIndicatesEncounteredErrorsNoMessagesSent()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse()
                    {
                        Status = TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent,
                        Error = new KeyValuePair<int, string>(1, "An error occurred."),
                        RawResponse = "test-response"
                    });

                var exception = AssertEx.TaskThrows<TransactClientException>(
                    () => new TransactClientTester(configuration: new TransactClientConfiguration()
                    {
                        PodNumber = 0
                    }, decoder: decoder).SendMessageAsync(new TransactMessage()));

                Assert.Equal("An error occurred.", exception.Message);
                Assert.Equal("XTMAILING", XDocument.Parse(exception.Request).Root.Name);
                Assert.Equal("test-response", exception.Response);
            }

            [Fact]
            public async Task PerformsHttpRequest()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).SendMessageAsync(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(
                        x => x.HttpUploadAsync(It.IsAny<string>(), /* tryRefreshingOAuthAccessToken: */ true),
                        Times.Once());
            }

            [Fact]
            public async Task ReturnsDecodedResponse()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse()
                    {
                        TransactionId = "123"
                    });

                var response = await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder).SendMessageAsync(new TransactMessage());

                Assert.Equal("123", response.TransactionId);
            }

            [Fact]
            public async Task DisposesISilverpopCommunicationsClient()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).SendMessageAsync(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(x => x.Dispose(), Times.Once());
            }
        }

        public class SendMessageBatchMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessages()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new TransactClientTester().SendMessageBatch(null));
            }

            [Fact]
            public void ThrowsWhenPodNumberIsNotConfigured()
            {
                var exception = Assert.Throws<ApplicationException>(
                    () => new TransactClientTester().SendMessageBatch(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingPodNumber, exception.Message);
            }

            [Fact, FreezeClock]
            public void ReturnsExpectedTrackingFilenames()
            {
                var filenames = new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, utcNow: Clock.UtcNow)
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipientsTwoBatches });

                Assert.Equal(Clock.UtcNow.ToString("s").Replace(':', '_') + "_UTC.1.xml.gz", filenames.First());
                Assert.Equal(Clock.UtcNow.ToString("s").Replace(':', '_') + "_UTC.2.xml.gz", filenames.Last());
                Assert.Equal(2, filenames.Count());
            }

            [Fact]
            public void PerformsSftpUpload()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, silverpopFactory: () => silverpop)
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipients })
                .Single();

                Mock.Get(silverpop)
                    .Verify(
                        x => x.SftpGzipUpload(It.IsAny<string>(), "transact/temp/" + filename),
                        Times.Once());
            }

            [Fact]
            public void PerformsSftpMove()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, silverpopFactory: () => silverpop)
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipients })
                .Single();

                // Moq.Sequences has a dependency on NUnit.
                // Rather than installing it to verify upload occurs before move
                // simply assuming the operations occur in the correct order.

                Mock.Get(silverpop)
                    .Verify(
                        x => x.SftpMove("transact/temp/" + filename, "transact/inbound/" + filename),
                        Times.Once());
            }

            [Fact]
            public void PerformsOperationsInBatches()
            {
                var encoder = Mock.Of<TransactMessageEncoder>();

                var filename = new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, encoder: encoder)
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipientsTwoBatches });

                Mock.Get(encoder)
                    .Verify(
                        x => x.Encode(It.IsAny<TransactMessage>()),
                        Times.Exactly(2));
            }

            [Fact]
            public void DisposesISilverpopCommunicationsClient()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).SendMessageBatch(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(x => x.Dispose(), Times.Once());
            }
        }

        public class SendMessageBatchAsyncMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessages()
            {
                AssertEx.TaskThrows<ArgumentNullException>(
                    () => new TransactClientTester().SendMessageBatchAsync(null));
            }

            [Fact]
            public void ThrowsWhenPodNumberIsNotConfigured()
            {
                var exception = AssertEx.TaskThrows<ApplicationException>(
                    () => new TransactClientTester().SendMessageBatchAsync(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingPodNumber, exception.Message);
            }

            [Fact, FreezeClock]
            public async Task ReturnsExpectedTrackingFilenames()
            {
                var filenames = await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, utcNow: Clock.UtcNow)
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipientsTwoBatches });

                Assert.Equal(Clock.UtcNow.ToString("s").Replace(':', '_') + "_UTC.1.xml.gz", filenames.First());
                Assert.Equal(Clock.UtcNow.ToString("s").Replace(':', '_') + "_UTC.2.xml.gz", filenames.Last());
                Assert.Equal(2, filenames.Count());
            }

            [Fact]
            public async Task PerformsSftpUpload()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = (await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, silverpopFactory: () => silverpop)
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipients }))
                .Single();

                Mock.Get(silverpop)
                    .Verify(
                        x => x.SftpGzipUploadAsync(It.IsAny<string>(), "transact/temp/" + filename),
                        Times.Once());
            }

            [Fact]
            public async Task PerformsSftpMove()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = (await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, silverpopFactory: () => silverpop)
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipients }))
                .Single();

                // Moq.Sequences has a dependency on NUnit.
                // Rather than installing it to verify upload occurs before move
                // simply assuming the operations occur in the correct order.

                Mock.Get(silverpop)
                    .Verify(
                        x => x.SftpMoveAsync("transact/temp/" + filename, "transact/inbound/" + filename),
                        Times.Once());
            }

            [Fact]
            public async Task PerformsOperationsInBatches()
            {
                var encoder = Mock.Of<TransactMessageEncoder>();

                var filename = (await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, encoder: encoder)
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipientsTwoBatches }));

                Mock.Get(encoder)
                    .Verify(
                        x => x.Encode(It.IsAny<TransactMessage>()),
                        Times.Exactly(2));
            }

            [Fact]
            public async Task DisposesISilverpopCommunicationsClient()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).SendMessageBatchAsync(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(x => x.Dispose(), Times.Once());
            }
        }

        public class GetStatusOfMessageBatchMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullFilename()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new TransactClientTester().GetStatusOfMessageBatch(null));
            }

            [Fact]
            public void ThrowsWhenPodNumberIsNotConfigured()
            {
                var exception = Assert.Throws<ApplicationException>(
                    () => new TransactClientTester().GetStatusOfMessageBatch("file.xml.gz"));

                Assert.Equal(TransactClient.ErrorMissingPodNumber, exception.Message);
            }

            [Fact]
            public void PerformsSftpDownload()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownload(It.IsAny<string>()))
                    .Returns(new MemoryStream());

                new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop)
                .GetStatusOfMessageBatch("file.xml.gz");

                Mock.Get(silverpop)
                    .Verify(
                        x => x.SftpDownload("transact/status/file.xml.gz"),
                        Times.Once());
            }

            [Fact]
            public void ThrowsWhenNoStatusFile()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownload(It.IsAny<string>()))
                    .Returns((Stream)null);

                var exception = Assert.Throws<TransactClientException>(
                    () => new TransactClientTester(configuration: new TransactClientConfiguration()
                    {
                        PodNumber = 0
                    }, silverpopFactory: () => silverpop)
                    .GetStatusOfMessageBatch("file.xml.gz"));

                Assert.Equal("file.xml.gz does not exist in the transact/status folder", exception.Message);
                Assert.Equal("transact/status/file.xml.gz", exception.Request);
                Assert.Null(exception.Response);
            }

            [Fact]
            public void ReturnsDecodedResponse()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse()
                    {
                        TransactionId = "123"
                    });

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownload(It.IsAny<string>()))
                    .Returns(new MemoryStream());

                var response = new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop)
                .GetStatusOfMessageBatch("file.xml.gz");

                Assert.Equal("123", response.TransactionId);
            }

            [Fact]
            public void DisposesISilverpopCommunicationsClient()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownload(It.IsAny<string>()))
                    .Returns(new MemoryStream());

                new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).GetStatusOfMessageBatch("file.xml.gz");

                Mock.Get(silverpop)
                    .Verify(x => x.Dispose(), Times.Once());
            }
        }

        public class GetStatusOfMessageBatchAsyncMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullFilename()
            {
                AssertEx.TaskThrows<ArgumentNullException>(
                    () => new TransactClientTester().GetStatusOfMessageBatchAsync(null));
            }

            [Fact]
            public void ThrowsWhenPodNumberIsNotConfigured()
            {
                var exception = AssertEx.TaskThrows<ApplicationException>(
                    () => new TransactClientTester().GetStatusOfMessageBatchAsync("file.xml.gz"));

                Assert.Equal(TransactClient.ErrorMissingPodNumber, exception.Message);
            }

            [Fact]
            public async Task PerformsSftpDownload()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownloadAsync(It.IsAny<string>()))
                    .ReturnsAsync(new MemoryStream());

                await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop)
                .GetStatusOfMessageBatchAsync("file.xml.gz");

                Mock.Get(silverpop)
                    .Verify(
                        x => x.SftpDownloadAsync("transact/status/file.xml.gz"),
                        Times.Once());
            }

            [Fact]
            public void ThrowsWhenNoStatusFile()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownloadAsync(It.IsAny<string>()))
                    .ReturnsAsync(null);

                var exception = AssertEx.TaskThrows<TransactClientException>(
                    () => new TransactClientTester(configuration: new TransactClientConfiguration()
                    {
                        PodNumber = 0
                    }, silverpopFactory: () => silverpop)
                    .GetStatusOfMessageBatchAsync("file.xml.gz"));

                Assert.Equal("file.xml.gz does not exist in the transact/status folder", exception.Message);
                Assert.Equal("transact/status/file.xml.gz", exception.Request);
                Assert.Null(exception.Response);
            }

            [Fact]
            public async Task ReturnsDecodedResponse()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse()
                    {
                        TransactionId = "123"
                    });

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownloadAsync(It.IsAny<string>()))
                    .ReturnsAsync(new MemoryStream());

                var response = await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop)
                .GetStatusOfMessageBatchAsync("file.xml.gz");

                Assert.Equal("123", response.TransactionId);
            }

            [Fact]
            public async Task DisposesISilverpopCommunicationsClient()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.SftpDownloadAsync(It.IsAny<string>()))
                    .ReturnsAsync(new MemoryStream());

                await new TransactClientTester(configuration: new TransactClientConfiguration()
                {
                    PodNumber = 0
                }, decoder: decoder, silverpopFactory: () => silverpop).GetStatusOfMessageBatchAsync("file.xml.gz");

                Mock.Get(silverpop)
                    .Verify(x => x.Dispose(), Times.Once());
            }
        }

        public class TransactClientTester : TransactClient
        {
            private readonly DateTime? _utcNow;

            public TransactClientTester(
                TransactClientConfiguration configuration = null,
                TransactMessageEncoder encoder = null,
                TransactMessageResponseDecoder decoder = null,
                Func<ISilverpopCommunicationsClient> silverpopFactory = null,
                DateTime? utcNow = null)
                : base(
                    configuration ?? new TransactClientConfiguration(),
                    encoder ?? new TransactMessageEncoder(),
                    decoder ?? new TransactMessageResponseDecoder(),
                    silverpopFactory ?? (() => Mock.Of<ISilverpopCommunicationsClient>()))
            {
                _utcNow = utcNow;
            }

            public override DateTime UtcNow
            {
                get { return _utcNow ?? base.UtcNow; }
            }
        }
    }
}