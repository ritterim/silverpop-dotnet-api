using System;
using Xunit;

namespace Silverpop.Client.Tests
{
    public class AccessTokenRefreshResponseTests
    {
        private readonly AccessTokenRefreshResponse _sut;

        public AccessTokenRefreshResponseTests()
        {
            _sut = new AccessTokenRefreshResponse();
        }

        public class ExpiresAtPropertyTests : AccessTokenRefreshResponseTests
        {
            [Fact]
            public void NullForDefaultExpiresIn()
            {
                Assert.Null(_sut.ExpiresAt);
            }

            [Fact]
            public void ThrowsForNegativeExpiresIn()
            {
                _sut.ExpiresIn = -1;

                Assert.Throws<ArgumentOutOfRangeException>(
                    () => _sut.ExpiresAt);
            }

            [Fact]
            public void ReturnsDateTimeInUtc()
            {
                _sut.ExpiresIn = 60;

                Assert.Equal(DateTimeKind.Utc, _sut.ExpiresAt.Value.Kind);
            }

            [Fact]
            public void ReturnsExpectedDateTime()
            {
                var utcNow = DateTime.UtcNow;

                var sutTester = new AccessTokenRefreshResponseTester(
                    utcNow: utcNow);

                sutTester.ExpiresIn = 60;

                Assert.Equal(utcNow.AddMinutes(1), sutTester.ExpiresAt);
            }
        }

        public class AccessTokenRefreshResponseTester : AccessTokenRefreshResponse
        {
            private readonly DateTime? _utcNow;

            public AccessTokenRefreshResponseTester(
                DateTime? utcNow = null)
            {
                _utcNow = utcNow ?? DateTime.UtcNow;
            }

            public override DateTime UtcNow
            {
                get { return _utcNow ?? base.UtcNow; }
            }
        }
    }
}