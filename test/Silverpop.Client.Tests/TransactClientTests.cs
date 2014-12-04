using AssertExLib;
using Moq;
using Silverpop.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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
            Enumerable.Range(0, TransactClient.MaxRecipientsPerBatchRequest + 1)
                .Select(x => new TransactMessageRecipient())
                .ToList();

        public static TransactClient Create(
            TransactClientConfiguration configuration = null,
            TransactMessageEncoder encoder = null,
            TransactMessageResponseDecoder decoder = null,
            ISilverpopCommunicationsClient silverpop = null)
        {
            return new TransactClient(
                configuration ?? new TransactClientConfiguration(),
                encoder ?? new TransactMessageEncoder(),
                decoder ?? new TransactMessageResponseDecoder(),
                silverpop ?? Mock.Of<ISilverpopCommunicationsClient>());
        }

        public class SendMessageMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessage()
            {
                Assert.Throws<ArgumentNullException>(
                    () => Create().SendMessage(null));
            }

            [Fact]
            public void ThrowsForTooManyRecipients()
            {
                var message = new TransactMessage()
                {
                    Recipients = Enumerable.Range(0, TransactClient.MaxRecipientsPerNonBatchRequest + 1)
                        .Select(x => new TransactMessageRecipient())
                        .ToList()
                };

                var exception = Assert.Throws<ArgumentException>(
                    () => Create().SendMessage(message));

                Assert.Equal(TransactClient.ErrorExceededNonBatchRecipients, exception.Message);
            }

            [Fact]
            public void ThrowsWhenTransactHttpUrlIsNotConfigured()
            {
                var exception = Assert.Throws<ApplicationException>(
                    () => Create().SendMessage(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingHttpUrl, exception.Message);
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
                        Error = new KeyValuePair<int, string>(1, "An error occurred.")
                    });

                var exception = Assert.Throws<TransactClientException>(
                    () => Create(configuration: new TransactClientConfiguration()
                          {
                              TransactHttpHost = "test-host"
                          }, decoder: decoder).SendMessage(new TransactMessage()));

                Assert.Equal("An error occurred.", exception.Message);
            }

            [Fact]
            public void PerformsHttpRequest()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                Create(configuration: new TransactClientConfiguration()
                {
                    TransactHttpHost = "test-host"
                }, decoder: decoder, silverpop: silverpop).SendMessage(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(
                        x => x.HttpUpload(It.IsAny<string>()),
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

                var response = Create(configuration: new TransactClientConfiguration()
                {
                    TransactHttpHost = "test-host"
                }, decoder: decoder).SendMessage(new TransactMessage());

                Assert.Equal("123", response.TransactionId);
            }
        }

        public class SendMessageAsyncMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessage()
            {
                AssertEx.TaskThrows<ArgumentNullException>(
                    () => Create().SendMessageAsync(null));
            }

            [Fact]
            public void ThrowsForTooManyRecipients()
            {
                var message = new TransactMessage()
                {
                    Recipients = Enumerable.Range(0, TransactClient.MaxRecipientsPerNonBatchRequest + 1)
                        .Select(x => new TransactMessageRecipient())
                        .ToList()
                };

                var exception = AssertEx.TaskThrows<ArgumentException>(
                    () => Create().SendMessageAsync(message));

                Assert.Equal(TransactClient.ErrorExceededNonBatchRecipients, exception.Message);
            }

            [Fact]
            public void ThrowsWhenTransactHttpUrlIsNotConfigured()
            {
                var exception = AssertEx.TaskThrows<ApplicationException>(
                    () => Create().SendMessageAsync(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingHttpUrl, exception.Message);
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
                        Error = new KeyValuePair<int, string>(1, "An error occurred.")
                    });

                var exception = AssertEx.TaskThrows<TransactClientException>(
                    () => Create(configuration: new TransactClientConfiguration()
                    {
                        TransactHttpHost = "test-host"
                    }, decoder: decoder).SendMessageAsync(new TransactMessage()));

                Assert.Equal("An error occurred.", exception.Message);
            }

            [Fact]
            public async Task PerformsHttpRequest()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                await Create(configuration: new TransactClientConfiguration()
                {
                    TransactHttpHost = "test-host"
                }, decoder: decoder, silverpop: silverpop).SendMessageAsync(new TransactMessage());

                Mock.Get(silverpop)
                    .Verify(
                        x => x.HttpUploadAsync(It.IsAny<string>()),
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

                var response = await Create(configuration: new TransactClientConfiguration()
                {
                    TransactHttpHost = "test-host"
                }, decoder: decoder).SendMessageAsync(new TransactMessage());

                Assert.Equal("123", response.TransactionId);
            }
        }

        public class SendMessageBatchMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessages()
            {
                Assert.Throws<ArgumentNullException>(
                    () => Create().SendMessageBatch(null));
            }

            [Fact]
            public void ThrowsWhenTransactFtpUrlIsNotConfigured()
            {
                var exception = Assert.Throws<ApplicationException>(
                    () => Create().SendMessageBatch(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingFtpUrl, exception.Message);
            }

            [Fact]
            public void ReturnsExpectedTrackingFilename()
            {
                var filename = Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                })
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipients })
                .Single();

                // filename is a randomly generated GUID with an .xml extension appended.

                Assert.True(filename.EndsWith(".xml"), "Filename should end in .xml");

                var filenameNoExtension = filename.Replace(".xml", string.Empty);

                Assert.DoesNotThrow(
                    () => Guid.Parse(filenameNoExtension));
            }

            [Fact]
            public void PerformsFtpUpload()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, silverpop: silverpop)
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipients })
                .Single();

                Mock.Get(silverpop)
                    .Verify(
                        x => x.FtpUpload(It.IsAny<string>(), "transact/temp/" + filename),
                        Times.Once());
            }

            [Fact]
            public void PerformsFtpMove()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, silverpop: silverpop)
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipients })
                .Single();

                // Moq.Sequences has a dependency on NUnit.
                // Rather than installing it to verify upload occurs before move
                // simply assuming the operations occur in the correct order.

                Mock.Get(silverpop)
                    .Verify(
                        x => x.FtpMove("transact/temp/" + filename, "transact/inbound/" + filename),
                        Times.Once());
            }

            [Fact]
            public void PerformsOperationsInBatches()
            {
                var encoder = Mock.Of<TransactMessageEncoder>();

                var filename = Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, encoder: encoder)
                .SendMessageBatch(new TransactMessage() { Recipients = TransactClientTests.TestRecipientsTwoBatches });

                Mock.Get(encoder)
                    .Verify(
                        x => x.Encode(It.IsAny<TransactMessage>()),
                        Times.Exactly(2));
            }
        }

        public class SendMessageBatchAsyncMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullMessages()
            {
                AssertEx.TaskThrows<ArgumentNullException>(
                    () => Create().SendMessageBatchAsync(null));
            }

            [Fact]
            public void ThrowsWhenTransactFtpUrlIsNotConfigured()
            {
                var exception = AssertEx.TaskThrows<ApplicationException>(
                    () => Create().SendMessageBatchAsync(new TransactMessage()));

                Assert.Equal(TransactClient.ErrorMissingFtpUrl, exception.Message);
            }

            [Fact]
            public async Task ReturnsExpectedTrackingFilename()
            {
                var filename = (await Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                })
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipients }))
                .Single();

                // filename is a randomly generated GUID with an .xml extension appended.

                Assert.True(filename.EndsWith(".xml"), "Filename should end in .xml");

                var filenameNoExtension = filename.Replace(".xml", string.Empty);

                Assert.DoesNotThrow(
                    () => Guid.Parse(filenameNoExtension));
            }

            [Fact]
            public async Task PerformsFtpUpload()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = (await Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, silverpop: silverpop)
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipients }))
                .Single();

                Mock.Get(silverpop)
                    .Verify(
                        x => x.FtpUploadAsync(It.IsAny<string>(), "transact/temp/" + filename),
                        Times.Once());
            }

            [Fact]
            public async Task PerformsFtpMove()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();

                var filename = (await Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, silverpop: silverpop)
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipients }))
                .Single();

                // Moq.Sequences has a dependency on NUnit.
                // Rather than installing it to verify upload occurs before move
                // simply assuming the operations occur in the correct order.

                Mock.Get(silverpop)
                    .Verify(
                        x => x.FtpMove("transact/temp/" + filename, "transact/inbound/" + filename),
                        Times.Once());
            }

            [Fact]
            public async Task PerformsOperationsInBatches()
            {
                var encoder = Mock.Of<TransactMessageEncoder>();

                var filename = (await Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, encoder: encoder)
                .SendMessageBatchAsync(new TransactMessage() { Recipients = TransactClientTests.TestRecipientsTwoBatches }));

                Mock.Get(encoder)
                    .Verify(
                        x => x.Encode(It.IsAny<TransactMessage>()),
                        Times.Exactly(2));
            }
        }

        public class GetStatusOfMessageBatchMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullFilename()
            {
                Assert.Throws<ArgumentNullException>(
                    () => Create().GetStatusOfMessageBatch(null));
            }

            [Fact]
            public void ThrowsWhenTransactFtpUrlIsNotConfigured()
            {
                var exception = Assert.Throws<ApplicationException>(
                    () => Create().GetStatusOfMessageBatch("file.xml"));

                Assert.Equal(TransactClient.ErrorMissingFtpUrl, exception.Message);
            }

            [Fact]
            public void PerformsFtpDownload()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.FtpDownload(It.IsAny<string>()))
                    .Returns(new MemoryStream());

                Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, decoder: decoder, silverpop: silverpop)
                .GetStatusOfMessageBatch("file.xml");

                Mock.Get(silverpop)
                    .Verify(
                        x => x.FtpDownload("transact/status/file.xml"),
                        Times.Once());
            }

            [Fact]
            public void ThrowsWhenNoStatusFile()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.FtpDownload(It.IsAny<string>()))
                    .Returns((Stream)null);

                var exception = Assert.Throws<TransactClientException>(
                    () => Create(configuration: new TransactClientConfiguration()
                    {
                        TransactFtpHost = "test-host"
                    }, silverpop: silverpop)
                    .GetStatusOfMessageBatch("file.xml"));

                Assert.Equal("file.xml does not exist in the transact/status folder", exception.Message);
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
                    .Setup(x => x.FtpDownload(It.IsAny<string>()))
                    .Returns(new MemoryStream());

                var response = Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, decoder: decoder, silverpop: silverpop)
                .GetStatusOfMessageBatch("file.xml");

                Assert.Equal("123", response.TransactionId);
            }
        }

        public class GetStatusOfMessageBatchAsyncMethodTests : TransactClientTests
        {
            [Fact]
            public void ThrowsForNullFilename()
            {
                AssertEx.TaskThrows<ArgumentNullException>(
                    () => Create().GetStatusOfMessageBatchAsync(null));
            }

            [Fact]
            public void ThrowsWhenTransactFtpUrlIsNotConfigured()
            {
                var exception = AssertEx.TaskThrows<ApplicationException>(
                    () => Create().GetStatusOfMessageBatchAsync("file.xml"));

                Assert.Equal(TransactClient.ErrorMissingFtpUrl, exception.Message);
            }

            [Fact]
            public async Task PerformsFtpDownload()
            {
                var decoder = Mock.Of<TransactMessageResponseDecoder>();
                Mock.Get(decoder)
                    .Setup(x => x.Decode(It.IsAny<string>()))
                    .Returns(new TransactMessageResponse());

                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.FtpDownloadAsync(It.IsAny<string>()))
                    .ReturnsAsync(new MemoryStream());

                await Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, decoder: decoder, silverpop: silverpop)
                .GetStatusOfMessageBatchAsync("file.xml");

                Mock.Get(silverpop)
                    .Verify(
                        x => x.FtpDownloadAsync("transact/status/file.xml"),
                        Times.Once());
            }

            [Fact]
            public void ThrowsWhenNoStatusFile()
            {
                var silverpop = Mock.Of<ISilverpopCommunicationsClient>();
                Mock.Get(silverpop)
                    .Setup(x => x.FtpDownloadAsync(It.IsAny<string>()))
                    .ReturnsAsync(null);

                var exception = AssertEx.TaskThrows<TransactClientException>(
                    () => Create(configuration: new TransactClientConfiguration()
                    {
                        TransactFtpHost = "test-host"
                    }, silverpop: silverpop)
                    .GetStatusOfMessageBatchAsync("file.xml"));

                Assert.Equal("file.xml does not exist in the transact/status folder", exception.Message);
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
                    .Setup(x => x.FtpDownloadAsync(It.IsAny<string>()))
                    .ReturnsAsync(new MemoryStream());

                var response = await Create(configuration: new TransactClientConfiguration()
                {
                    TransactFtpHost = "test-host"
                }, decoder: decoder, silverpop: silverpop)
                .GetStatusOfMessageBatchAsync("file.xml");

                Assert.Equal("123", response.TransactionId);
            }
        }
    }
}