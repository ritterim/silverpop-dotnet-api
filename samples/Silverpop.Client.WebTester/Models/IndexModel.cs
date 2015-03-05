namespace Silverpop.Client.WebTester.Models
{
    public class IndexModel
    {
        public IndexModel()
        {
        }

        public IndexModel(TransactClientConfiguration config)
        {
            PodNumber = config.PodNumber;
            Username = config.Username;
            OAuthClientId = config.OAuthClientId;
        }

        public int? PodNumber { get; set; }

        public string Username { get; set; }

        public string OAuthClientId { get; set; }

        public string AuthenticationType
        {
            get
            {
                return string.IsNullOrEmpty(OAuthClientId)
                    ? "Regular"
                    : "OAuth";
            }
        }

        public string AuthenticationLogin
        {
            get
            {
                if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(OAuthClientId))
                    return "[Not Configured]";

                if (string.IsNullOrEmpty(OAuthClientId))
                    return Username;

                // Truncate OAuthClientId on UI
                return OAuthClientId.Length >= 6
                    ? OAuthClientId.Substring(0, 6) + "..."
                    : OAuthClientId;
            }
        }
    }
}