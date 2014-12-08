using System;
using Xunit;

namespace Silverpop.Client.Tests
{
    public class TransactClientConfigurationTests
    {
        private readonly TransactClientConfiguration _sut;

        public TransactClientConfigurationTests()
        {
            _sut = new TransactClientConfiguration();
        }

        public class TransactHttpsUrlPropertyTests : TransactClientConfigurationTests
        {
            [Fact]
            public void KeepsExistingHttpsScheme()
            {
                _sut.TransactHttpsUrl = "https://test";

                Assert.Equal("https://test", _sut.TransactHttpsUrl);
            }

            [Fact]
            public void DoesNotThrowForNull()
            {
                _sut.TransactHttpsUrl = "https://test";

                Assert.DoesNotThrow(() => _sut.TransactHttpsUrl = null);
            }

            [Fact]
            public void ThrowsWhenHttpsSchemeWhenOmitted()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => _sut.TransactHttpsUrl = "test");

                Assert.Equal(
                    "TransactHttpsUrl property must begin with \"https://\".",
                    exception.Message);
            }

            [Fact]
            public void ThrowsWhenUrlHasIncorrectScheme()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => _sut.TransactHttpsUrl = "http://test");

                Assert.Equal(
                    "TransactHttpsUrl property must begin with \"https://\".",
                    exception.Message);
            }
        }

        public class TransactSftpUrlPropertyTests : TransactClientConfigurationTests
        {
            [Fact]
            public void KeepsExistingSftpScheme()
            {
                _sut.TransactSftpUrl = "sftp://test";

                Assert.Equal("sftp://test", _sut.TransactSftpUrl);
            }

            [Fact]
            public void DoesNotThrowForNull()
            {
                _sut.TransactSftpUrl = "sftp://test";

                Assert.DoesNotThrow(() => _sut.TransactSftpUrl = null);
            }

            [Fact]
            public void ThrowsWhenSftpSchemeWhenOmitted()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => _sut.TransactSftpUrl = "test");

                Assert.Equal(
                    "TransactSftpUrl property must begin with \"sftp://\".",
                    exception.Message);
            }

            [Fact]
            public void ThrowsWhenUrlHasIncorrectScheme()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => _sut.TransactSftpUrl = "ftp://test");

                Assert.Equal(
                    "TransactSftpUrl property must begin with \"sftp://\".",
                    exception.Message);
            }
        }
    }
}