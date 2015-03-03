using System.Configuration;

namespace Silverpop.Client
{
    public class TransactClientConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("podNumber")]
        public int PodNumber { get { return (int)this["podNumber"]; } }

        [ConfigurationProperty("username")]
        public string Username { get { return (string)this["username"]; } }

        [ConfigurationProperty("password")]
        public string Password { get { return (string)this["password"]; } }

        [ConfigurationProperty("oAuthClientId")]
        public string OAuthClientId { get { return (string)this["oAuthClientId"]; } }

        [ConfigurationProperty("oAuthClientSecret")]
        public string OAuthClientSecret { get { return (string)this["oAuthClientSecret"]; } }

        [ConfigurationProperty("oAuthRefreshToken")]
        public string OAuthRefreshToken { get { return (string)this["oAuthRefreshToken"]; } }

        public static TransactClientConfigurationSection GetFromConfiguration()
        {
            return ConfigurationManager.GetSection("transactClientConfiguration")
                as TransactClientConfigurationSection;
        }
    }
}