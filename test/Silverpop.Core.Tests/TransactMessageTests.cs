using System;
using System.Linq;
using Xunit;

namespace Silverpop.Core.Tests
{
    public class TransactMessageTests
    {
        private readonly TransactMessage _sut;

        public TransactMessageTests()
        {
            _sut = new TransactMessage();
        }

        public class GetRecipientBatchedMessagesMethodTests : TransactMessageTests
        {
            [Fact]
            public void ThrowsForMaxRecipientsPerMessageOfNegative1()
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => _sut.GetRecipientBatchedMessages(-1));
            }

            [Fact]
            public void ThrowsForMaxRecipientsPerMessageOf0()
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => _sut.GetRecipientBatchedMessages(0));
            }

            [Fact]
            public void PerformsOneBatchWithMaxRecipientsPerMessageOf1()
            {
                _sut.Recipients.Add(
                    new TransactMessageRecipient() { EmailAddress = "test@example.com" });

                var batches = _sut.GetRecipientBatchedMessages(1);

                Assert.Equal(1, batches.Count());
            }

            [Fact]
            public void PerformsOneBatchWithMaxRecipientsPerMessageOf2()
            {
                _sut.Recipients.Add(
                    new TransactMessageRecipient() { EmailAddress = "test@example.com" });

                var batches = _sut.GetRecipientBatchedMessages(2);

                Assert.Equal(1, batches.Count());
            }

            [Fact]
            public void PerformsMoreThanOneBatch()
            {
                _sut.Recipients.Add(
                    new TransactMessageRecipient() { EmailAddress = "test1@example.com" });

                _sut.Recipients.Add(
                    new TransactMessageRecipient() { EmailAddress = "test2@example.com" });

                var batches = _sut.GetRecipientBatchedMessages(1);

                Assert.Equal(2, batches.Count());
            }

            [Fact]
            public void DoesNotPermanentlyModifyRecipientsOneBatch()
            {
                _sut.Recipients.Add(
                    new TransactMessageRecipient() { EmailAddress = "test@example.com" });

                var batches = _sut.GetRecipientBatchedMessages(2);

                Assert.Equal("test@example.com", _sut.Recipients.Single().EmailAddress);
            }

            [Fact]
            public void DoesNotPermanentlyModifyRecipientsMoreThanOneBatch()
            {
                _sut.Recipients.Add(
                    new TransactMessageRecipient() { EmailAddress = "test1@example.com" });

                _sut.Recipients.Add(
                    new TransactMessageRecipient() { EmailAddress = "test2@example.com" });

                var batches = _sut.GetRecipientBatchedMessages(1);

                Assert.Equal("test1@example.com", _sut.Recipients.First().EmailAddress);
                Assert.Equal("test2@example.com", _sut.Recipients.Last().EmailAddress);
            }
        }
    }
}