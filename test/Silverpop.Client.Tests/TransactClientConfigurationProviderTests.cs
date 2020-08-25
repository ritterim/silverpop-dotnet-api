using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Silverpop.Client.Tests
{
    public class TransactClientConfigurationProviderTests
    {
#if NETCOREAPP
        [Fact]
        public void GetFromAppSettingsUsingIConfiguration()
        {
            var appSettings = new Dictionary<string, string>
            {
                ["test-prefix:podNumber"] = "1",
                ["test-prefix:username"] = "test-username",
                ["test-prefix:password"] = "test-password",
                ["test-prefix:oAuthClientId"] = "test-oAuthClientId",
                ["test-prefix:oAuthClientSecret"] = "test-oAuthClientSecret",
                ["test-prefix:oAuthRefreshToken"] = "test-oAuthRefreshToken",
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();

            var sut = new TransactClientConfigurationProvider("test-prefix:");

            var settings = sut.GetFromAppSettings(configuration);

            Assert.Equal(1, settings.PodNumber);
            Assert.Equal("test-username", settings.Username);
            Assert.Equal("test-password", settings.Password);
            Assert.Equal("test-oAuthClientId", settings.OAuthClientId);
            Assert.Equal("test-oAuthClientSecret", settings.OAuthClientSecret);
            Assert.Equal("test-oAuthRefreshToken", settings.OAuthRefreshToken);
        }
#endif
    }
}
