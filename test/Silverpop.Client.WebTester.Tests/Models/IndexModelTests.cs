using Silverpop.Client.WebTester.Models;
using Xunit;

namespace Silverpop.Client.WebTester.Tests.Models
{
    public class IndexModelTests
    {
        public class AuthenticationTypeTests
        {
            [Fact]
            public void RegularWhenOAuthClientIsNotSpecified()
            {
                var model = new IndexModel();

                Assert.Equal("Regular", model.AuthenticationType);
            }

            [Fact]
            public void OAuthWhenOAuthClientIsSpecified()
            {
                var model = new IndexModel() { OAuthClientId = "some_oAuthClientId" };

                Assert.Equal("OAuth", model.AuthenticationType);
            }
        }

        public class AuthenticationLoginTests
        {
            [Fact]
            public void DisplaysCorrectNotConfiguredValue()
            {
                var model = new IndexModel();

                Assert.Equal("[Not Configured]", model.AuthenticationLogin);
            }

            [Fact]
            public void UsernameWhenOAuthClientIdIsNotSpecified()
            {
                var model = new IndexModel() { Username = "some_username" };

                Assert.Equal("some_username", model.AuthenticationLogin);
            }

            [Fact]
            public void OAuthWhenOAuthClientIdIsSpecified()
            {
                var model = new IndexModel() { OAuthClientId = "my_id" };

                Assert.Equal("my_id", model.AuthenticationLogin);
            }

            public void DoesNotTruncateOAuthClientIdValueOf6Characters()
            {
                var model = new IndexModel() { OAuthClientId = "123456" };

                Assert.Equal("123456", model.AuthenticationLogin);
            }

            [Fact]
            public void TruncatesOAuthClientIdValueGreaterThan6Characters()
            {
                var model = new IndexModel() { OAuthClientId = "123456789" };

                Assert.Equal("123456...", model.AuthenticationLogin);
            }
        }
    }
}