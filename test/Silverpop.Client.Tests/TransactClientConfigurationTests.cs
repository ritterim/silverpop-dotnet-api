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
                _sut.PodNumber = 0;
            }

            [Fact]
            public void DoesNotThrowForPositiveNumber()
            {
                _sut.PodNumber = 1;
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

        public class HydrateUsingMethodTests : TransactClientConfigurationTests
        {
            private const int TestPodNumber = 2;
            private const string TestUsername = "some-username";
            private const string TestPassword = "some-password";
            private const string TestOAuthClientId = "some-oauth-client-id";
            private const string TestOAuthClientSecret = "some-oauth-client-secret";
            private const string TestOAuthRefreshToken = "some-oauth-refresh-token";

            [Fact]
            public void ThrowsForNullConfiguration()
            {
                Assert.Throws<ArgumentNullException>(
                    () => _sut.HydrateUsing(null));
            }

            [Fact]
            public void UsesPodNumberValue()
            {
                var config = new TransactClientConfiguration
                {
                    PodNumber = TestPodNumber,
                };

                _sut.HydrateUsing(config);

                Assert.Equal(TestPodNumber, _sut.PodNumber);
            }

            [Fact]
            public void UsesUsernameValue()
            {
                var config = new TransactClientConfiguration
                {
                    Username = TestUsername,
                };

                _sut.HydrateUsing(config);

                Assert.Equal(TestUsername, _sut.Username);
            }

            [Fact]
            public void UsesPasswordValue()
            {
                var config = new TransactClientConfiguration
                {
                    Password = TestPassword,
                };

                _sut.HydrateUsing(config);

                Assert.Equal(TestPassword, _sut.Password);
            }

            [Fact]
            public void UsesOAuthClientIdValue()
            {
                var config = new TransactClientConfiguration
                {
                    OAuthClientId = TestOAuthClientId,
                };

                _sut.HydrateUsing(config);

                Assert.Equal(TestOAuthClientId, _sut.OAuthClientId);
            }

            [Fact]
            public void UsesOAuthClientSecretValue()
            {
                var config = new TransactClientConfiguration
                {
                    OAuthClientSecret = TestOAuthClientSecret,
                };

                _sut.HydrateUsing(config);

                Assert.Equal(TestOAuthClientSecret, _sut.OAuthClientSecret);
            }

            [Fact]
            public void UsesOAuthRefreshTokenValue()
            {
                var config = new TransactClientConfiguration
                {
                    OAuthRefreshToken = TestOAuthRefreshToken,
                };

                _sut.HydrateUsing(config);

                Assert.Equal(TestOAuthRefreshToken, _sut.OAuthRefreshToken);
            }

            [Fact]
            public void DoesNotModifyExistingPodNumber()
            {
                _sut.PodNumber = TestPodNumber;

                _sut.HydrateUsing(new TransactClientConfiguration());

                Assert.Equal(TestPodNumber, _sut.PodNumber);
            }

            [Fact]
            public void DoesNotModifyExistingUsername()
            {
                _sut.Username = TestUsername;

                _sut.HydrateUsing(new TransactClientConfiguration());

                Assert.Equal(TestUsername, _sut.Username);
            }

            [Fact]
            public void DoesNotModifyExistingPassword()
            {
                _sut.Password = TestPassword;

                _sut.HydrateUsing(new TransactClientConfiguration());

                Assert.Equal(TestPassword, _sut.Password);
            }

            [Fact]
            public void DoesNotModifyExistingOAuthClientId()
            {
                _sut.OAuthClientId = TestOAuthClientId;

                _sut.HydrateUsing(new TransactClientConfiguration());

                Assert.Equal(TestOAuthClientId, _sut.OAuthClientId);
            }

            [Fact]
            public void DoesNotModifyExistingOAuthClientSecret()
            {
                _sut.OAuthClientSecret = TestOAuthClientSecret;

                _sut.HydrateUsing(new TransactClientConfiguration());

                Assert.Equal(TestOAuthClientSecret, _sut.OAuthClientSecret);
            }

            [Fact]
            public void DoesNotModifyExistingOAuthRefreshToken()
            {
                _sut.OAuthRefreshToken = TestOAuthRefreshToken;

                _sut.HydrateUsing(new TransactClientConfiguration());

                Assert.Equal(TestOAuthRefreshToken, _sut.OAuthRefreshToken);
            }
        }
    }
}