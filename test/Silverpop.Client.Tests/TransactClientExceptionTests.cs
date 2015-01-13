using Xunit;

namespace Silverpop.Client.Tests
{
    public class TransactClientExceptionTests
    {
        public class RequestOnlyConstructorTests
        {
            private readonly TransactClientException _sut =
                new TransactClientException("test-message", "test-request");

            [Fact]
            public void SetsExceptionMessage()
            {
                Assert.Equal("test-message", _sut.Message);
            }

            [Fact]
            public void SetsRequestProperty()
            {
                Assert.Equal("test-request", _sut.Request);
            }

            [Fact]
            public void DoesNotSetResponseProperty()
            {
                Assert.Null(_sut.Response);
            }
        }

        public class RequestResponseConstructorTests
        {
            private readonly TransactClientException _sut =
                new TransactClientException("test-message", "test-request", "test-response");

            [Fact]
            public void SetsExceptionMessage()
            {
                Assert.Equal("test-message", _sut.Message);
            }

            [Fact]
            public void SetsRequestProperty()
            {
                Assert.Equal("test-request", _sut.Request);
            }

            [Fact]
            public void SetsResponseProperty()
            {
                Assert.Equal("test-response", _sut.Response);
            }
        }
    }
}