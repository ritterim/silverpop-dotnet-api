using System;
using System.Configuration;
using System.Linq;

namespace Silverpop.Client
{
    public class TransactClientConfigurationProvider
    {
        private const string DefaultAppSettingsPrefix = "silverpop-dotnet-api:";

        private readonly string _appSettingsPrefix;

        /// <summary>
        /// Construct using the DefaultAppSettingsPrefix.
        /// </summary>
        public TransactClientConfigurationProvider()
            : this(DefaultAppSettingsPrefix)
        {
        }

        /// <summary>
        /// Construct with a custom prefix.
        /// Include any seperator characters as part of the prefix.
        /// </summary>
        public TransactClientConfigurationProvider(
            string appSettingsPrefix)
        {
            _appSettingsPrefix = appSettingsPrefix;
        }

        public virtual TransactClientConfiguration GetFromAppSettings()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Any(
                    x => x.StartsWith(_appSettingsPrefix, StringComparison.Ordinal)))
            {
                return null;
            }

            int podNumber;
            var parseablePodNumberSet = int.TryParse(
                GetAppSettingValueOrNull("PodNumber"),
                out podNumber);

            var config = new TransactClientConfiguration
            {
                PodNumber = parseablePodNumberSet ? podNumber : (int?)null,
                Username = GetAppSettingValueOrNull("Username"),
                Password = GetAppSettingValueOrNull("Password"),
                OAuthClientId = GetAppSettingValueOrNull("OAuthClientId"),
                OAuthClientSecret = GetAppSettingValueOrNull("OAuthClientSecret"),
                OAuthRefreshToken = GetAppSettingValueOrNull("OAuthRefreshToken")
            };

            return config;
        }

        public virtual TransactClientConfiguration GetFromConfigurationSection()
        {
            var configSection = ConfigurationManager.GetSection("transactClientConfiguration")
                as TransactClientConfigurationSection;

            if (configSection == null)
            {
                return null;
            }

            var config = new TransactClientConfiguration
            {
                PodNumber = configSection.PodNumber,
                Username = GetValueOrNull(configSection.Username),
                Password = GetValueOrNull(configSection.Password),
                OAuthClientId = GetValueOrNull(configSection.OAuthClientId),
                OAuthClientSecret = GetValueOrNull(configSection.OAuthClientSecret),
                OAuthRefreshToken = GetValueOrNull(configSection.OAuthRefreshToken)
            };

            return config;
        }

        private string GetAppSettingValueOrNull(string keyNoPrefix)
        {
            return GetValueOrNull(ConfigurationManager.AppSettings[_appSettingsPrefix + keyNoPrefix]);
        }

        private static string GetValueOrNull(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }
    }
}