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

        public class PodNumberPropertyTests : TransactClientConfigurationTests
        {
            [Fact]
            public void IsNullBeforeSet()
            {
                Assert.Null(_sut.PodNumber);
            }

            [Fact]
            public void DoesNotThrowForZero()
            {
                Assert.DoesNotThrow(() => _sut.PodNumber = 0);
            }

            [Fact]
            public void DoesNotThrowForPositiveNumber()
            {
                Assert.DoesNotThrow(() => _sut.PodNumber = 1);
            }

            [Fact]
            public void ThrowsWhenNegativePodNumberSet()
            {
                var exception = Assert.Throws<ArgumentOutOfRangeException>(
                    () => _sut.PodNumber = -1);

                Assert.Equal(
                    "PodNumber must not be a negative number." +
                    Environment.NewLine +
                    "Parameter name: PodNumber",
                    exception.Message);
            }
        }
    }
}